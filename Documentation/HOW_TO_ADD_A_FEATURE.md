# How to add a feature (beginner guide)

This guide walks you through adding a new feature to the backend,
**file by file**, even if you are not comfortable with OOP yet.

Before you start, open the existing `Example*` files and keep them
side by side. You will mostly **copy that pattern** and rename it.

Useful commands are listed in `commands.md`.

If your feature needs the database, start the **dev PostgreSQL** first
with `Backend.DAL/Dockerfile` (see the Docker section in `commands.md`
or the README).

---

## Tiny OOP vocabulary (only what you need)

| Word | Simple meaning |
| --- | --- |
| **Class** | A blueprint for an object (data + behavior). Example: `ExampleService` |
| **Object / instance** | One concrete use of a class, created by the app when needed |
| **Property** | A piece of data on a class (`Name`, `Id`, …) |
| **Method** | A function that belongs to a class |
| **Interface** (`ISomething`) | A contract: “this type must provide these methods”. Useful for testing and DI |
| **Dependency Injection (DI)** | The app builds and gives you the objects you need (service, controller, …) instead of you writing `new` everywhere |
| **Layer** | A project with one job (API, business logic, database, models) |

You do **not** need to invent a new architecture. Follow the layers below.

---

## Big picture: what happens when someone calls your API

```
Browser / frontend
        │
        ▼
1. Route          (URL + HTTP method + CancellationToken)
                                          Backend.API/Routes
        │
        ▼
2. Controller     (glue: DTO → service, forward token)
                                          Backend.API/Controllers
        │
        ▼
3. Service        (business rules + forward token)
                                          Backend.BLL/Services
        │                                 (throw AppException on expected failures)
        ▼
4. Repository     (DB + pass token to EF) Backend.DAL/Interfaces + Repositories
        │                                 ← only if you save data
        ▼
5. Model          (shape of the data)     Backend.Models

On error (any layer, uncaught):
        │
        ▼
   GlobalExceptionHandler                 Backend.API
   → ProblemDetails JSON (401 / 404 / 409 / … or 500)
```

Rule of thumb:

- **Route** = “where is the door?” (+ request `CancellationToken`)
- **DTO** = “what data is allowed to enter?” (**DataAnnotations**, French messages)
- **Controller** = “who do I call next?” (forward the token)
- **Service** = “what is the real work?” (+ expected error cases + token)
- **Repository** = “how do I read/write the database?” (token → EF)
- **Model** = “what does this data look like inside the app?”
- **AppException** = “this failure is expected; map it to a proper HTTP status”
- **GlobalExceptionHandler** = “turn uncaught exceptions into ProblemDetails”
- **CancellationToken** = “stop work if the client disconnects” (BCL type;
  safe in BLL/DAL — not an ASP.NET leak)

---

## Choose a feature name

Pick **one** PascalCase name and reuse it everywhere.

Examples: `Device`, `Home`, `User`, `Scene`

In this guide we use **`Device`**.  
Replace `Device` with your own name in every file.

---

## Checklist

```
[ ] Step 1  — Model
[ ] Step 2  — Repository (+ database + CancellationToken on async I/O)
              [skip if no DB yet]
[ ] Step 3  — Service interface (CancellationToken on async I/O)
[ ] Step 4  — Service implementation (+ AppException for expected errors;
              forward CancellationToken)
[ ] Step 5  — Register the service (BLL DI)
[ ] Step 6  — DTOs with DataAnnotations + French ErrorMessage (mandatory)
[ ] Step 7  — Controller (forward CancellationToken)
[ ] Step 8  — Register the controller (API DI)
[ ] Step 9  — Routes (+ CancellationToken + RequireAuthorization when private
              + .ProducesValidationProblem)
[ ] Step 10 — Map the routes in Program.cs
[ ] Step 11 — Review expected error paths + document them in Swagger
              (.Produces / .ProducesProblem on routes)
[ ] Step 12 — Build and test happy path + error paths
              (Bearer JWT in Swagger if protected; bad DTO → 400)
```

Work **from the bottom of the stack upward** (model → … → route).
That way each file can already use the previous one.

---

## Step 1 — Model

**File to create:** `Backend.Models/DeviceModels.cs`

**Role:** describe the data. No logic. Only properties.

Look at: `Backend.Models/ExampleModels.cs`

Minimal shape:

```csharp
namespace Backend.Models;

public class Device {
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
```

Notes for beginners:

- `{ get; set; }` means “this value can be read and written”
- `required` means “you must set this when creating the object”
- `Guid` is a unique id (search “C# Guid” if needed)

---

## Step 2 — Repository and database (only if you persist data)

Skip this whole step if your feature does not touch the database yet
(like the current `Example` feature).

### 2.1 Register the model in EF (required to persist it)

**File to edit:** `Backend.DAL/Database.cs` (`AppDbContext`)

Creating a C# model is not enough. You **must** register it on
`AppDbContext` so Entity Framework:

- maps it to a database table
- includes it in migrations

Add a `DbSet` so EF knows the table exists, for example:

```csharp
public DbSet<Device> Devices => Set<Device>();
```

Add relationships in `OnModelCreating` only when you need them
(see the commented example already in that file).

Connection strings live under `DALConfig` in
`appsettings.Development.json` (dev Docker DB on port `4040`) and
`appsettings.Production.json` (fake prod example). Do not hardcode them.
Start the DB with `Backend.DAL/Dockerfile` before running migrations
(see `commands.md`).

### 2.2 Repository interface

**File to create:** `Backend.DAL/Interfaces/IDeviceRepository.cs`

**Role:** list the database operations the rest of the app is allowed to use.
Same idea as `Backend.BLL/Interfaces` for services.

```csharp
using Backend.Models;

namespace Backend.DAL.Interfaces;

public interface IDeviceRepository {
    Task<Device?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken
    );
    Task AddAsync(Device device, CancellationToken cancellationToken);
}
```

Prefer returning `Device?` (or empty lists) when nothing is found.
Let the **service** decide whether that is a `NotFoundException` or
another business rule.

Pass `cancellationToken` into EF (`FirstOrDefaultAsync`,
`SaveChangesAsync`, …). `CancellationToken` is from `System.Threading`
(the .NET BCL), not ASP.NET — it does **not** break layering.

### 2.3 Repository implementation

**File to create:** `Backend.DAL/Repositories/DeviceRepository.cs`

**Role:** real SQL/EF code. Only this layer talks to `AppDbContext`.

The class should implement `IDeviceRepository` and receive `AppDbContext`
through its constructor (DI will provide it). Forward
`cancellationToken` to every cancellable EF call.

### 2.4 Register the repository

**File to edit:** `Backend.DAL/DependencyInjection.cs`

Inside `AddDAL()`, add:

```csharp
services.AddScoped<IDeviceRepository, DeviceRepository>();
```

Meaning in plain words:
“When someone asks for `IDeviceRepository`, give them a `DeviceRepository`.”

### 2.5 Migration

When the model is ready, create and apply a migration
(see `commands.md`):

```bash
dotnet ef migrations add AddDevice --project backend.dal --startup-project backend.api
dotnet ef database update --project backend.dal --startup-project backend.api
```

---

## Step 3 — Service interface

**File to create:** `Backend.BLL/Interfaces/IDeviceService.cs`

**Role:** declare what the business layer can do, without saying how.

Look at: `Backend.BLL/Interfaces/IExampleService.cs`

```csharp
using Backend.Models;

namespace Backend.BLL.Interfaces;

public interface IDeviceService {
    Task<Device> GetDeviceAsync(
        string name,
        CancellationToken cancellationToken
    );
}
```

Why an interface?

- The controller depends on `IDeviceService`, not on the concrete class
- Later you can mock it in tests without touching the database
- Async I/O methods take `CancellationToken` and forward it downward

---

## Step 4 — Service implementation

**File to create:** `Backend.BLL/Services/DeviceService.cs`

**Role:** business logic (rules, calculations, orchestration).

Look at: `Backend.BLL/Services/ExampleService.cs`

```csharp
using Backend.Models;
using Backend.Models.Exceptions;
using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;

namespace Backend.BLL.Services;

public class DeviceService(
    IDeviceRepository _deviceRepository
) : IDeviceService {
    public async Task<Device> GetDeviceAsync(
        string name,
        CancellationToken cancellationToken
    ) {
        Device? device = await _deviceRepository.GetByNameAsync(
            name,
            cancellationToken
        );

        if (device is null) {
            throw new NotFoundException("Appareil introuvable.");
        }

        return device;
    }
}
```

If you have a repository, inject it in the constructor (as above) and
call it from your methods. Always forward `cancellationToken`.

**Expected errors:** list what can go wrong for the user (not found,
duplicate name, bad credentials, …). For each case, throw an
`AppException` subtype from `Backend.Models.Exceptions` with a
**French** message (shown in the frontend):

| Exception | HTTP status | Example message |
| --- | --- | --- |
| `UnauthorizedException` | 401 | `"Mot de passe invalide."` |
| `NotFoundException` | 404 | `"Appareil introuvable."` |
| `ConflictException` | 409 | `"Email déjà utilisé."` |

You do **not** catch these in the controller. They bubble up to
`GlobalExceptionHandler`, which returns ProblemDetails JSON.

Rules:

- Put business decisions here
- Do **not** read HTTP details here (no DTOs from the API project)
- Do **not** use `AppDbContext` directly here (use the repository)
- Do **not** throw bare `new Exception("…")` for expected cases
  (that becomes a 500)

---

## Step 5 — Register the service

**File to edit:** `Backend.BLL/DependencyInjection.cs`

Inside `AddBLL()`, add:

```csharp
services.AddScoped<IDeviceService, DeviceService>();
```

If you forget this line, the app may compile but crash at runtime with
“Unable to resolve service…”.

---

## Step 6 — DTOs (DataAnnotations are mandatory)

**File to create:** `Backend.API/DTOs/DeviceDTOs.cs`

**Role:** validate and shape data that enters (or leaves) the HTTP API.

Look at: `Backend.API/DTOs/ExampleDTOs.cs` and `AuthDTOs.cs`.

**Do not ship a request DTO as a bare property bag.** Many AI-generated
DTOs skip annotations — that is wrong in this project. Every constrained
field needs DataAnnotations, with **French** `ErrorMessage`.

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.DTOs;

public sealed class GetDeviceDTO {
    [FromQuery]
    [Required(ErrorMessage = "Le nom est requis.")]
    [MinLength(1, ErrorMessage = "Le nom ne doit pas être vide.")]
    public string Name { get; init; } = string.Empty;
}

public sealed class CreateDeviceDTO {
    [Required(ErrorMessage = "Le nom est requis.")]
    [MinLength(2, ErrorMessage = "Le nom doit contenir au moins 2 caractères.")]
    public string Name { get; set; } = string.Empty;
}
```

Notes:

- A **DTO** is not your database model. It is the “form” of the request/response
- Use `[Required]`, `[MinLength]`, `[EmailAddress]`, `[RegularExpression]`, …
- `ErrorMessage` values shown to the user stay in **French**
- Shape/format checks → DTO annotations; DB business rules → BLL
  `AppException`
- `builder.Services.AddValidation();` is already in `Program.cs`
- Routes that bind a validated DTO need `.ProducesValidationProblem()`

---

## Step 7 — Controller

**File to create:** `Backend.API/Controllers/DeviceController.cs`

**Role:** thin glue between the route and the service.

Look at: `Backend.API/Controllers/ExampleController.cs`

```csharp
using Backend.API.DTOs;
using Backend.Models;
using Backend.BLL.Interfaces;

namespace Backend.API.Controllers;

public class DeviceController(
    IDeviceService _deviceService
) {
    public Task<Device> GetDeviceAsync(
        GetDeviceDTO dto,
        CancellationToken cancellationToken
    ) {
        return _deviceService.GetDeviceAsync(dto.Name, cancellationToken);
    }
}
```

What this file should do:

- Take a validated DTO and the request `CancellationToken`
- Call one (or a few) service methods (forward the token)
- Return the result

What it should **not** do:

- Contain heavy business rules
- Talk to the database / `AppDbContext`
- Swallow the token (always pass it down)

---

## Step 8 — Register the controller

**File to edit:** `Backend.API/DependencyInjection.cs`

Inside `AddAPI()`, add:

```csharp
services.AddScoped<DeviceController>();
```

Same idea as the service: without this line, the route cannot receive
the controller from DI.

---

## Step 9 — Routes

**File to create:** `Backend.API/Routes/DeviceRoute.cs`

**Role:** expose the HTTP endpoint(s).

Look at: `Backend.API/Routes/ExampleRoutes.cs`

**REST naming:** route paths must stay **REST**. REST
(Representational State Transfer) means the URL names a **resource**
(a noun, usually plural) and the HTTP method names the **action**:

| Method | Path | Meaning |
| --- | --- | --- |
| `GET` | `/devices` | List devices |
| `GET` | `/devices/{id}` | Get one device |
| `POST` | `/devices` | Create a device |
| `PUT` / `PATCH` | `/devices/{id}` | Replace / update |
| `DELETE` | `/devices/{id}` | Delete a device |

Avoid verb paths like `/getDevice` or `/createDevice`. Nested resources
are fine when ownership is clear (`/homes/{homeId}/rooms`). The Example
`/hello_world` path is only a demo.

```csharp
using Backend.API.Controllers;
using Backend.API.DTOs;
using Backend.Models;

namespace Backend.API.Routes;

public static class DeviceRoutes {
    public static void MapDeviceRoutes(this IEndpointRouteBuilder mainRouter) {
        var deviceRouter = mainRouter.MapGroup("/devices");

        deviceRouter.MapGet("/", async (
            DeviceController controller,
            [AsParameters] GetDeviceDTO dto,
            CancellationToken cancellationToken
        ) => {
            return await controller.GetDeviceAsync(dto, cancellationToken);
        })
        .RequireAuthorization("User")
        .Produces<Device>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
```

Bind `CancellationToken cancellationToken` on the handler and pass it
to the controller (ASP.NET supplies the request-abort token).

Document each **expected** status in Swagger with `.Produces` /
`.ProducesValidationProblem` (invalid DTO → 400) /
`.ProducesProblem` (ProblemDetails from `GlobalExceptionHandler`).
Include `401` / `403` when the route uses `.RequireAuthorization(...)`.
`AddValidation()` is already registered in `Program.cs`.

**Auth (JWT Bearer):** protected routes need
`.RequireAuthorization("User")` or `"Admin"`. The client sends
`Authorization: Bearer <access_token>` (token from
`POST /api/auth/login` or `POST /api/auth/register`). There is
**no external verification server** — the API validates the JWT
locally and checks that the `jti` is still active (logout revokes
it via `POST /api/auth/logout`). Current user: `GET /api/auth/me`.
Token lifetime: `JwtConfig.ExpirationTime` minutes. Expired
`UserToken` rows are cleaned by `TokenCleanupBackgroundService`.
CORS allows the SPA at `http://localhost:8080`. See `DAT.md` §9 /
`AGENTS.md` (Authentication).

Important binding rule:

- On a **GET**, a complex DTO must use `[AsParameters]`
  so ASP.NET reads query params (`?name=lamp`) instead of a JSON body
- GET requests do not have a body

Example URL after this step:

`GET /devices?name=lamp`

---

## Step 10 — Wire the routes in Program.cs

**File to edit:** `Backend.API/Program.cs`

1. Make sure the using exists:

```csharp
using Backend.API.Routes;
```

2. After the other `Map…Routes()` calls, add:

```csharp
app.MapDeviceRoutes();
```

`AddAPI()` is already called in `Program.cs`. It chains `AddBLL()` →
`AddDAL()` for you. You usually only add a new `Map…` line here for a
new feature (and register the new types in each layer’s
`DependencyInjection.cs`).

---

## Step 11 — Review expected error paths

Before testing, write down the failure cases for your feature and check
that the service throws the right `AppException` subtype. Then document
those statuses on the route with `.Produces` / `.ProducesProblem` so
Swagger shows them.

Examples:

- Get missing id → `NotFoundException` → client sees **404** ProblemDetails
- Create duplicate → `ConflictException` → **409**
- Bad login → `UnauthorizedException` → **401** (French message)
- Protected route without / with revoked token → **401** / **403**

Reference files:

- `Backend.Models/Exceptions/AppException.cs`
- `Backend.API/GlobalExceptionHandler.cs`
- `Backend.BLL/Services/AuthService.cs` (login credentials)
- `Backend.API/Routes/AuthRoutes.cs` (Swagger `.ProducesProblem` examples)

You should **not** add a new exception handler per feature. Reuse the
global one. Only add a new exception **type** if none of the existing
subtypes fit.

---

## Step 12 — Build and test

```bash
dotnet build
dotnet run --project backend.api
```

API listens on `http://localhost:5050` with path base `/api`.
Open Swagger (development) and try your new route under `/api/...`.

If the route is protected: call `POST /api/auth/login` (or register),
copy `access_token`, authorize in Swagger with the `bearer` scheme
(paste the JWT **without** the `Bearer` prefix), then call your route.

Also test at least one **error path** (wrong id, bad body, …) and check
that the status code is not a blind 500 when the failure was expected.

If something fails, check these common mistakes first:

| Symptom | Likely cause |
| --- | --- |
| `Unable to resolve service for type '…'` | Missing line in a `DependencyInjection.cs` |
| `Body was inferred but the method does not allow…` | Missing `[AsParameters]` on a GET DTO |
| Project / namespace errors | Wrong folder or forgotten `using` |
| Validation never rejects bad input | Missing DataAnnotations on the DTO, or `AddValidation()` missing in `Program.cs` (already present in this project) |
| Bad body still reaches the service | DTO has no `[Required]` / constraints — add annotations + French `ErrorMessage` |
| Client abort still runs DB work | `CancellationToken` not forwarded route → … → EF |
| `401 Unauthorized` | Missing / expired Bearer token, or route not authorized |
| `403 Forbidden` | Valid token but wrong role for the policy |
| Expected business error returns `500` | Threw bare `Exception` instead of an `AppException` subtype |

---

## File map (one feature = these places)

For a feature named `Device`, you typically touch:

```
Backend.Models/
  DeviceModels.cs
  Exceptions/AppException.cs          (shared — reuse, rarely edit)

Backend.DAL/                          (only if persistence)
  Database.cs                         (edit AppDbContext)
  Interfaces/IDeviceRepository.cs
  Repositories/DeviceRepository.cs
  DependencyInjection.cs              (edit)

Backend.BLL/
  Interfaces/IDeviceService.cs
  Services/DeviceService.cs           (throw AppException subtypes)
  DependencyInjection.cs              (edit)

Backend.API/
  DTOs/DeviceDTOs.cs
  Controllers/DeviceController.cs
  Routes/DeviceRoute.cs
  GlobalExceptionHandler.cs           (shared — reuse, rarely edit)
  DependencyInjection.cs              (edit)
  Program.cs                          (edit: MapDeviceRoutes)
```

---

## How to add typed configuration (`XXXConfig.cs`)

When a project needs settings (connection string, feature flags, …):

1. Create `<Layer>Config.cs` in that project (see `Backend.DAL/DALConfig.cs`)
2. Add DataAnnotations (`[Required]`, …) on properties you must validate
3. Add the JSON section in `Backend.API/appsettings.json` and override per
   environment in `appsettings.Development.json` /
   `appsettings.Production.json`
4. Bind it in that project’s `DependencyInjection.cs`:

```csharp
services.AddOptions<DALConfig>()
    .BindConfiguration("DALConfig")
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

5. Inject `IOptions<DALConfig>` (or your config type) where you need the values

The JSON section name must match the string passed to `BindConfiguration`.

---

## Golden rules (short)

1. Copy `Example*`, then rename carefully
2. One feature name, reused in every layer
3. Never skip layers (Controller must not call the repository directly)
4. Always register new services/controllers/repositories in DI
5. Persist models by declaring them on `AppDbContext` in `Database.cs`
6. Keep route names REST (resource nouns + HTTP verbs)
7. Protect private routes with `.RequireAuthorization(...)` (Bearer JWT)
8. Throw `AppException` subtypes for expected errors (not bare `Exception`)
9. **Validate request DTOs** with DataAnnotations + French `ErrorMessage`
10. **Propagate `CancellationToken`** route → controller → service → repo → EF
11. Keep each file focused on its job
12. Write code and comments in **English**; user-facing errors in **French**

If you are stuck, open the matching `Example*` / `Auth*` file for that
layer and mirror it line by line with your feature name.
More detail: `AGENTS.md` (CancellationToken + DTO validation sections).
