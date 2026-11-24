# AGENTS.md

Instructions for AI agents working on this backend. Follow them for every change.

## Project

Alfred — home automation backend (SI & Infra).

- Stack: .NET 10, C#, Minimal APIs, EF Core, PostgreSQL
- Auth: JWT Bearer (local validation, no introspect server)
- Architecture: N-tier (layered)
- Reference feature: `Example*` across all layers

All project content (code, comments, docs, commit messages) must be
**English**. **Exception:** user-facing error and validation messages
(DTO `ErrorMessage`, `AppException` titles/details, generic 500 text)
are in **French** for the frontend.

API path base: **`/api`** (`UsePathBase("/api")`). Clients call
`http://localhost:5050/api/...`.

---

## Authentication (JWT Bearer)

Auth is **JWT Bearer**. There is **no external verification /
introspection server**. The API issues the JWT on login **or register**
and validates it locally on each protected request. Active tokens are
tracked by `jti` in `UserToken` so logout can revoke them early.

**Client flow** (paths relative to `/api`)

1. `POST /auth/register` — create an account and receive
   `{ access_token, token_type: "Bearer" }` (same shape as login)
2. `POST /auth/login` — receive `{ access_token, token_type: "Bearer" }`
   (JWT `jti` persisted in `UserToken`)
3. Send on protected routes:
   `Authorization: Bearer <access_token>`
4. `GET /auth/me` (requires `User`) — current user profile
5. `POST /auth/logout` (requires `User`) — revoke the current token by
   deleting its `jti` from the database

The token expires after `JwtConfig.ExpirationTime` minutes
(default `60`). After expiry or logout, the client must log in again.

**Revocation checks**

- `ActiveJtiHandler` ensures the JWT `jti` still exists before
  authorization succeeds (logged-out tokens are rejected)
- Do not bypass this by parsing tokens manually in controllers /
  services

**Token cleanup daemon**

- `TokenCleanupBackgroundService` is a hosted `BackgroundService`
  registered in `Backend.API` DI
- It runs with the API process and every 5 minutes deletes expired
  `UserToken` rows via `IAuthService.CleanupExpiredTokensAsync()`
- Keep cleanup business logic in the BLL; the hosted service only
  schedules and opens a DI scope

**Protecting routes**

```csharp
.RequireAuthorization("User")   // roles User or Admin
.RequireAuthorization("Admin")  // role Admin only
```

Public auth routes (`/auth/register`, `/auth/login`) stay without
`.RequireAuthorization(...)`. `/auth/me` and `/auth/logout` are
protected (`User`). JWT middleware is already wired in `Program.cs`
(`UseAuthentication` / `UseAuthorization`). Do not reimplement
token parsing in controllers or services.

**Config** — `JwtConfig` in `appsettings*.json`; signing `Key` from
user-secrets / env (never commit it). Details: `DAT.md` §9.

**CORS** — policy `Frontend` from `Cors:AllowedOrigins` (dev:
`http://localhost:8080`). Wired with `AddCors` / `UseCors` in
`Program.cs`. Keep origins aligned with the Angular serve port.

---

## Architecture (mandatory)

```
HTTP request
    ↓
Backend.API        Routes → Controllers → DTOs
                   (+ GlobalExceptionHandler)
    ↓
Backend.BLL        Interfaces (I*Service) → Services
                   (throw AppException for expected errors)
    ↓
Backend.DAL        Interfaces (I*Repository) → Repositories → AppDbContext
    ↓
Backend.Models     Shared domain models + Exceptions
```

### Projects and responsibilities

| Project | Role | May contain |
| --- | --- | --- |
| `Backend.API` | HTTP edge | Routes, Controllers, DTOs, `GlobalExceptionHandler`, API DI |
| `Backend.BLL` | Business logic | `Interfaces/`, `Services/`, BLL DI |
| `Backend.DAL` | Persistence | `Interfaces/`, `Repositories/`, `AppDbContext`, DAL DI |
| `Backend.Models` | Shared models | POCOs with properties only, `Exceptions/` |

### Dependency rules (do not break)

Allowed project references:

- `API` → `BLL`, `Models`
- `BLL` → `DAL`, `Models`
- `DAL` → `Models`
- `Models` → nothing in this solution

Forbidden:

- `API` calling `AddDAL()` or `AddBLL()` directly from `Program.cs`
- `BLL` or `DAL` importing API types (DTOs, Controllers, Routes)
- `Models` depending on any other project
- Putting business logic in Routes or DTOs
- Accessing `AppDbContext` from API or Controllers (repositories only)
- Skipping a layer (e.g. Controller calling a Repository directly)

DI registration is **chained** so each layer pulls in the layer below it:

```
Program.cs → AddAPI() → AddBLL() → AddDAL()
```

`Program.cs` only calls `AddAPI()` and maps routes. Each layer’s
`DependencyInjection.cs` owns its own registrations and invokes the
next layer’s `Add*()` method.

### Request flow

1. **Route** (`Backend.API/Routes`) — exposes the HTTP endpoint, binds input
   (validated DTO) + `CancellationToken`, injects the controller
2. **Controller** (`Backend.API/Controllers`) — maps DTO → service call,
   forwards the same `CancellationToken`
3. **Service** (`Backend.BLL/Services` via interface) — business rules;
   throws `AppException` subtypes for expected failures; forwards the token
4. **Repository** (`Backend.DAL/Repositories` via `Backend.DAL/Interfaces`) —
   database access with the token on EF async APIs (prefer returning
   `null` / empty over throwing business errors)
5. **Model** (`Backend.Models`) — shared shape of domain data
6. **On error** — uncaught exceptions bubble to `GlobalExceptionHandler`
   (`IExceptionHandler`): `AppException` → matching HTTP status +
   ProblemDetails; anything else → 500

---

## CancellationToken (mandatory on async I/O)

Propagate `CancellationToken` through the full stack on async work that
touches I/O (DB, HTTP, long awaits):

```
Route → Controller → Service (interface + impl) → Repository (interface + impl)
  → EF / HttpClient / other cancellable APIs
```

Rules:

- Accept `CancellationToken cancellationToken` on the route handler,
  controller method, service method, and repository method
- Pass **the same** token down; do not create a new token per layer
- Pass it into EF Core (`ToListAsync`, `FirstOrDefaultAsync`,
  `SaveChangesAsync`, `AnyAsync`, `ExecuteDeleteAsync`, …) and other
  cancellable APIs
- `CancellationToken` is `System.Threading` (BCL), **not** ASP.NET.
  Putting it on BLL/DAL signatures does **not** break layering. Do
  **not** pass `HttpContext` / `IHttpContextAccessor` into BLL or DAL
- Sync or trivially CPU-only helpers may omit it; every new async
  feature path with I/O must include it
- Background services use their own `stoppingToken` (see
  `TokenCleanupBackgroundService`), not the HTTP request token

Why: if the client disconnects, work should stop instead of keeping
orphan DB/HTTP work alive.

---

## DTO validation with DataAnnotations (mandatory)

**Do not skip this.** Many agents ship bare DTO properties with no
attributes. In this project, request DTOs **must** validate input with
DataAnnotations on the properties.

Rules:

- Put validation on **API DTOs** (`Backend.API/DTOs`), not by reinventing
  the same checks only inside the service
- Annotate every required / constrained field:
  `[Required]`, `[MinLength]`, `[MaxLength]`, `[EmailAddress]`,
  `[RegularExpression]`, `[Range]`, …
- Set **`ErrorMessage` in French** on each attribute (frontend display)
- Prefer `sealed` request/response DTOs when inheritance is unused
- Keep `builder.Services.AddValidation();` in `Program.cs` (already
  present) so Minimal APIs reject invalid bodies/queries with 400
- Document invalid input on routes with `.ProducesValidationProblem()`
- Business rules that need DB state (duplicate email, …) stay in the
  BLL as `AppException` — annotations cover **shape / format** of the
  request

Example (see `AuthDTOs.cs` / `ExampleDTOs.cs`):

```csharp
public sealed class RegisterRequestDTO {
    [Required(ErrorMessage = "L'email est requis !")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide !")]
    public string Email { get; set; } = string.Empty;
}
```

GET query DTOs still need annotations **and** `[AsParameters]` on the
route parameter (and `[FromQuery]` on properties when needed).

---

## Error handling

Expected business failures must **not** become opaque 500 responses.
Use the shared exception hierarchy and the global handler.

### Pieces

| Piece | Location | Role |
| --- | --- | --- |
| `AppException` (+ subtypes) | `Backend.Models/Exceptions/` | Expected errors with `StatusCode` + `Title` |
| `GlobalExceptionHandler` | `Backend.API/GlobalExceptionHandler.cs` | Maps exceptions → ProblemDetails JSON |
| Registration | `AddExceptionHandler` + `AddProblemDetails` in API DI; `app.UseExceptionHandler()` early in `Program.cs` | Wires the pipeline |

Existing subtypes:

| Type | HTTP | Typical use |
| --- | --- | --- |
| `UnauthorizedException` | 401 | Bad credentials, missing auth context |
| `NotFoundException` | 404 | Resource does not exist |
| `ConflictException` | 409 | Duplicate / state conflict |

Add a new subtype under `Backend.Models/Exceptions/` only when none of
the above fits (keep the HTTP status in the constructor, as integers —
Models must not reference ASP.NET).

### Rules

- **Throw from the BLL** (services) for expected business failures:
  `throw new NotFoundException("Appareil introuvable.");`
- Prefer repositories that return `T?` / empty collections; let the
  service decide which `AppException` to throw
- Do **not** `try/catch` in every controller/route just to map status
  codes — the global handler already does that
- Do **not** throw bare `Exception` / `InvalidOperationException` for
  expected cases (those become 500)

**`AppException` messages are user-facing (not like a 500):**

- The handler puts the exception **message** (and title) into
  ProblemDetails. The frontend often **displays that text** to the user
- Write those strings carefully: clear, polite, **French**, safe to show
- Do **not** put stack traces, SQL, internal ids, English debug notes,
  or developer jargon in `AppException` messages
- Prefer short product copy, e.g.
  `throw new NotFoundException("Appareil introuvable.");`
  or login: `"Mot de passe invalide."`
- Unexpected bugs stay uncaught → handler logs + a **generic** 500
  ProblemDetails in French (no stack trace / no internal detail to the
  client). Do not rely on 500 text to explain a business failure —
  use the right `AppException` subtype instead

### Feature work

When implementing a feature, list expected failure modes (not found,
conflict, forbidden business rule, …) and throw the matching
`AppException` subtype from the service. Smoke-test those paths as well
as the happy path.

Reference: `AuthService.Login` → `UnauthorizedException`; handler in
`GlobalExceptionHandler`.

---

## Naming conventions

| Kind | Convention | Example |
| --- | --- | --- |
| Types, public members, files, folders, projects, namespaces | PascalCase | `DeviceService`, `Backend.BLL.Services` |
| Interfaces | `I` prefix | `IDeviceService` |
| Async methods | `Async` suffix | `GetDeviceAsync()` |
| Booleans | `Is` / `Has` / `Can` | `IsActive` |
| Parameters and locals | camelCase | `deviceId` |
| Private fields / primary-ctor deps | `_camelCase` | `_deviceService` |

Feature naming: use one feature prefix everywhere (`Device`, `Home`, `User`, …). Mirror the `Example*` layout.

**Extend an existing feature when the work is tightly related.** A new
capability is not always a new feature prefix / new file set. If it
belongs to the same domain aggregate, add methods, DTOs, routes, and
models **inside** that feature instead of creating parallel
`Invitation*` / `HomeInvite*` stacks.

Example: a **home invitation code** is part of **Home** — extend
`HomeService`, `HomeController`, `HomeRoutes`, `HomeDTOs`,
`IHomeRepository`, models under Home, etc. Do **not** invent a full
`Invitation` feature tree just to limit conceptual “newness”; the goal
is fewer files and one clear owner.

Create a **new** feature prefix only when the domain is clearly
separate (e.g. `Device` vs `Home`, `Auth` vs `Scene`).

---

## How to add a feature

Use the `Example*` feature as the template. Complete the steps that apply; skip persistence steps only when the feature has no database work.

**Before step 1:** decide whether this is a **new** feature or an
**extension** of an existing one (see Feature naming above). If it
extends an existing feature, edit those files — do not copy the whole
checklist into a new parallel file set.

### Checklist

Copy and track:

```
Feature: <Name>

[ ] 1. Model(s) in Backend.Models
[ ] 2. Repository interface in Backend.DAL/Interfaces (if persistence)
[ ] 3. Repository class in Backend.DAL/Repositories (if persistence)
[ ] 4. Register entity on AppDbContext + constraints (if persistence)
[ ] 5. Register repository in Backend.DAL/DependencyInjection.cs
[ ] 6. EF migration (if persistence)
[ ] 7. Service interface in Backend.BLL/Interfaces
       (async I/O methods take CancellationToken)
[ ] 8. Service implementation in Backend.BLL/Services
       (throw AppException subtypes for expected errors;
        forward CancellationToken)
[ ] 9. Register service in Backend.BLL/DependencyInjection.cs
[ ] 10. Request/response DTOs in Backend.API/DTOs
        (DataAnnotations + French ErrorMessage on every constrained field)
[ ] 11. Controller in Backend.API/Controllers
        (forward CancellationToken to the service)
[ ] 12. Register controller in Backend.API/DependencyInjection.cs
[ ] 13. Routes in Backend.API/Routes + Map* in Program.cs
        (bind CancellationToken; .ProducesValidationProblem on DTOs)
[ ] 14. RequireAuthorization on private routes (User / Admin)
[ ] 15. Expected errors covered (AppException / existing subtypes)
       + documented on routes with .Produces / .ProducesProblem
[ ] 16. Build and smoke-test happy path + error paths
       (Bearer token in Swagger when protected; bad DTO → 400)
```

### Step details

#### 1. Models — `Backend.Models/<Feature>Models.cs`

- Properties only (no business logic, no EF attributes unless required later)
- Shared across layers intentionally (avoid duplicating the same type per layer)
- Shared exceptions live in `Backend.Models/Exceptions/` (do not put
  feature-specific HTTP mapping in Models)

#### 2–6. Persistence — `Backend.DAL` (when needed)

- Interface: `I<Feature>Repository` in `Interfaces/` (same pattern as BLL)
- Implementation: `<Feature>Repository` in `Repositories/`
- Prefer `Task<T?>` / empty results over throwing business exceptions here
- Async methods that hit EF must take `CancellationToken` and pass it to
  EF APIs (`FirstOrDefaultAsync(..., cancellationToken)`, …)
- **Query style (required):** compose with **EF Core fluent method
  syntax** on `DbSet` (`Where` / `OrderBy` / `ThenBy` / `Select` /
  `ToArrayAsync`, …). Project in the query; pass `cancellationToken`
  on the terminal async call. Prefer this over raw SQL strings and
  over LINQ query syntax (`from x in … select`). Do not load full
  entities into memory then filter with LINQ-to-Objects when EF can
  do it. See Coding style → Repository queries.
- **Register the model in `AppDbContext` (`Database.cs`)** — required for EF to create the table and keep it in sync via migrations:
  - Add `public DbSet<YourModel> YourModels => Set<YourModel>();`
  - Declare relationships / constraints in `OnModelCreating` when needed
  - Without this step the model exists in code but is **not** persisted
- Register: `services.AddScoped<I<Feature>Repository, <Feature>Repository>();`
- Migration commands: see `commands.md`
- Connection string comes from `DALConfig` (`ConnectionString` +
  `DBPassword` / `{PASSWORD_PLACEHOLDER}`). Prod password via env
  `DALConfig__DBPassword` — see `commands.md`

#### 7–9. Business logic — `Backend.BLL`

- Interface: `I<Feature>Service` in `Interfaces/`
- Implementation: `<Feature>Service` in `Services/`, depends on repository interfaces (not concrete types)
- Async I/O methods take `CancellationToken` and forward it to the repo
- Register: `services.AddScoped<I<Feature>Service, <Feature>Service>();`
- For each expected failure, `throw` an `AppException` subtype
  (`NotFoundException`, `ConflictException`, `UnauthorizedException`, …).
  The API global handler maps it to ProblemDetails — no local try/catch
  required in the controller

#### 10–13. HTTP edge — `Backend.API`

**DTOs** (`DTOs/<Feature>DTOs.cs`) — **required, do not omit:**

- Validate **every** constrained input field with DataAnnotations
  (`[Required]`, `[MinLength]`, `[EmailAddress]`, …)
- Always set **French** `ErrorMessage` on those attributes
- Prefer dedicated `sealed` request/response DTOs; do not expose
  internal wiring through the API contract when a response DTO is clearer
- Do not leave a request DTO as a bare property bag with no annotations
- `builder.Services.AddValidation();` is already in `Program.cs`

**Controller** (`Controllers/<Feature>Controller.cs`):

- Inject `I<Feature>Service` (interface, not concrete class)
- Accept DTOs + `CancellationToken`, call the service, return the result
- Forward the token to the service; no direct database access

**DI** (`DependencyInjection.cs`):

- `services.AddScoped<<Feature>Controller>();`

**Routes** (`Routes/<Feature>Route.cs`):

- Static class with `Map<Feature>Routes(this IEndpointRouteBuilder …)`
- Use `MapGroup("/<feature>")` with a **REST** resource name (noun, usually plural)
- Inject the controller from DI; keep handlers thin
- Bind `CancellationToken cancellationToken` on async handlers and
  pass it to the controller
- Call `app.Map<Feature>Routes();` from `Program.cs`
- Protect private endpoints with `.RequireAuthorization("User")` or
  `"Admin"` (JWT Bearer — see Authentication section above)
- Document expected responses for Swagger: `.Produces` for success,
  `.ProducesValidationProblem()` when the route binds a validated DTO,
  `.ProducesProblem` for each expected error status (`AppException`
  subtypes + `401`/`403` on protected routes)

**REST naming (required for real features):**

REST (Representational State Transfer) means the URL names a **resource**
(a noun), and the HTTP method names the **action**. Prefer:

| Method | Path | Meaning |
| --- | --- | --- |
| `GET` | `/devices` | List devices |
| `GET` | `/devices/{id}` | Get one device |
| `POST` | `/devices` | Create a device |
| `PUT` / `PATCH` | `/devices/{id}` | Replace / partially update |
| `DELETE` | `/devices/{id}` | Delete a device |

Avoid verb paths like `/getDevices` or `/createDevice`. Nested resources are
fine when ownership is clear (`/homes/{homeId}/rooms`). The `Example` route
(`/example/hello_world`) is a demo only.

**Binding rules (Minimal APIs):**

- Complex query DTOs on `MapGet` → mark with `[AsParameters]` (GET has no body)
- Body DTOs on `MapPost` / `MapPut` / `MapPatch` → body binding is fine
- `[FromQuery]` on a property alone does not make a complex parameter a query model

### What not to do when adding a feature

- Do not create a full new feature file set when the capability is
  deeply tied to an existing feature (e.g. home invitation code →
  extend `Home*`, do not add `Invitation*` everywhere)
- Do not put service or repository logic inside route lambdas
- Do not register concrete services where an interface already exists (`IExampleService` pattern)
- Do not add a new project or flatten layers without an explicit architectural decision
- Do not hardcode secrets; connection strings and JWT `Key` belong in
  `appsettings*.json` / user-secrets / env
- Do not leave a new type unregistered in its layer’s `DependencyInjection.cs`
- Do not skip registering a persisted model on `AppDbContext` in `Database.cs`
- Do not leave private routes open; add `.RequireAuthorization(...)`
- Do not add an external JWT introspection / verification server; validation
  stays local in the API
- Do not ship request DTOs **without** DataAnnotations + French
  `ErrorMessage` (shape/format checks belong on the DTO)
- Do not stop `CancellationToken` at the route/controller: forward it to
  service → repository → EF / HTTP
- Do not inject `HttpContext` into BLL/DAL “instead of” a token
- Do not write repository access as raw SQL or LINQ query syntax when
  a fluent EF chain (`Where` / `OrderBy` / `Select` / `To*Async`) works
- Do not leave non-obvious features/methods uncommented — match the
  `Example*` file-header / intent comment style (English)
- Do not throw bare `Exception` for expected business failures (use
  `AppException` subtypes so clients get the right status, not 500)
- Do not put unsafe / internal / English debug text in `AppException`
  messages — they are often shown to the user (unlike a generic 500)
- Do not catch exceptions in every controller just to write HTTP status
  codes — rely on `GlobalExceptionHandler`
- Do not reintroduce `UseDeveloperExceptionPage` for this API unless you
  accept HTML stack traces instead of ProblemDetails in Development

---

## Typed configuration (`XXXConfig.cs`)

Each project that needs settings should own a typed config class.

### Steps

1. **Create** `<Layer>Config.cs` in that project (example: `Backend.DAL/DALConfig.cs`)
2. **Annotate** properties with DataAnnotations (`[Required]`, …) so bad/missing
   values fail fast at startup
3. **Add** the matching JSON section in `Backend.API/appsettings.json` and the
   environment overlays:
   - `appsettings.Development.json` → local / dev values
   - `appsettings.Production.json` → production values (fake placeholders in
     the repo; real secrets stay out of git)
4. **Bind + validate** in that project’s `DependencyInjection.cs`:

```csharp
services.AddOptions<DALConfig>()
    .BindConfiguration("DALConfig")
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

5. **Consume** via DI with `IOptions<T>` (or `IOptionsSnapshot<T>` / `IOptionsMonitor<T>` when needed):

```csharp
public class SomeService(IOptions<DALConfig> dalConfig) {
    private readonly string _connectionString =
        dalConfig.Value.ConnectionString!;
}
```

The section name in JSON must match `BindConfiguration("…")`
(e.g. `"DALConfig"` → class `DALConfig`).

### Current connection strings

| File | Purpose |
| --- | --- |
| `appsettings.json` | Default / Azure host string with `{PASSWORD_PLACEHOLDER}` |
| `appsettings.Development.json` | Local PostgreSQL (dev Docker DB) + `DBPassword` |
| `appsettings.Production.json` | Fake production example |

Section shape:

```json
"DALConfig": {
  "ConnectionString": "…;Password={PASSWORD_PLACEHOLDER};…",
  "DBPassword": "…"
}
```

`DALConfig.GetFullConnectionString()` replaces `{PASSWORD_PLACEHOLDER}`
with `DBPassword` before Npgsql connects.

| Environment | Where `DBPassword` comes from |
| --- | --- |
| Development | `appsettings.Development.json` (local Docker password) |
| Production | env var `DALConfig__DBPassword` (never commit the real value) |

Optional local override: user-secrets
`dotnet user-secrets set "DALConfig:DBPassword" "…"`. Details:
`commands.md`.

### JWT settings (`JwtConfig`)

```json
"JwtConfig": {
  "Issuer": "alfred_backend",
  "Audience": "alfred_backend",
  "ExpirationTime": 60
}
```

`Key` (Base64 signing key) must come from user-secrets or environment
variables — not from tracked appsettings. See `commands.md`.

### Dev database (Docker)

Use `Backend.DAL/Dockerfile` for local PostgreSQL (**dev only** —
password is tracked in git). Build/run commands: see `commands.md`.

Default settings: host `localhost`, port `4040`, database `alfred_db`
(user/password in the Dockerfile). Keep
`appsettings.Development.json` aligned with those values
(`ConnectionString` placeholder + `DBPassword`).

---

## Dependency injection

Each layer owns its registrations and chains the layer below:

| File | Calls next | Registers |
| --- | --- | --- |
| `Backend.API/DependencyInjection.cs` | `AddBLL()` | controllers, exception handler, `TokenCleanupBackgroundService` |
| `Backend.BLL/DependencyInjection.cs` | `AddDAL()` | services (`I*Service` → `*Service`) |
| `Backend.DAL/DependencyInjection.cs` | — | `AppDbContext`, repositories |

Default lifetime: **Scoped**.

`Program.cs` only registers the API layer (the rest is chained):

```csharp
builder.Services.AddAPI();
```

Inside the layers:

```csharp
// AddAPI
services.AddBLL();
services.AddScoped<ExampleController>();

// AddBLL
services.AddDAL();
services.AddScoped<IExampleService, ExampleService>();

// AddDAL
services.AddOptions<DALConfig>()
    .BindConfiguration("DALConfig")
    .ValidateDataAnnotations()
    .ValidateOnStart();
services.AddDbContext<AppDbContext>(...);
// services.AddScoped<IRepository, Repository>();
```

---

## Coding style for this repo

- Prefer primary constructors for Controllers/Services when dependencies are simple
- Prefer interfaces for BLL and DAL collaborators (testability / mocking)
- Prefer `sealed` on DTOs when inheritance is not needed
- Annotate request DTO fields with DataAnnotations + French `ErrorMessage`
- Propagate `CancellationToken` on async I/O (route → … → repository)
- Keep routes as extension methods on `IEndpointRouteBuilder`
- Match existing file headers and section comments style when editing nearby code
- Keep user-facing validation / error messages in **French**;
  everything else in English
- Treat `AppException` messages as **UI copy** (user may see them);
  reserve internal detail for logs / real 500s only

### Comments (when intent is not obvious)

Comment features and non-obvious functions in **English**, in the same
spirit as the `Example*` files (file-level `/** … */` that explain
**why** / role of the piece, not a line-by-line rewrite of the code).

- Add a short file header on new feature files (Routes, DTOs, Services,
  Repositories, …) when the purpose is not obvious from the name alone
- Comment methods / blocks when the rule, edge case, or design choice
  is non-obvious
- Do **not** comment the trivial (`// increment i`, getters, obvious
  DI wiring)
- Prefer the existing section markers (`// Imports`, `// Class`, …)
  when editing nearby code

Reference tone: `ExampleRoutes.cs`, `ExampleDTOs.cs`,
`ExampleService.cs`.

### Repository queries (EF Core fluent style)

In repositories, write queries as **fluent method chains** on
`DbSet` / `IQueryable`. Keep filtering, ordering, and projection in
the database query; finish with an async materializer and the
`CancellationToken`.

**Prefer:**

```csharp
await db.HomeMemberships
    .Where(m => m.HomeId == homeId && homesForUser.Contains(m.HomeId))
    .OrderBy(m => m.User.Username)
    .ThenBy(m => m.User.Id)
    .Select(m => new HomeMember(m.User.Username, m.Role))
    .ToArrayAsync(cancellationToken);
```

**Avoid:**

- Raw SQL (`FromSqlRaw`, `ExecuteSqlRaw`, hand-written SQL strings)
  unless there is a clear, documented reason
- LINQ **query syntax** (`from m in db.HomeMemberships where … select`)
- Loading a full list then filtering/sorting in memory when EF can
  express it (`ToList()` then `.Where(...)` on the client)

---


## Commands

See `commands.md` for Docker (dev DB), restore, build, run, watch,
and EF migrations.

Minimum verification after a feature:

```bash
# start the dev database first (see commands.md)
dotnet build
dotnet run --project backend.api
```

---

## When unsure

1. Copy the structure of the closest `Example*` / `Auth*` files
2. Respect the dependency table above
3. Prefer a thin route + controller + interface service over shortcuts
4. Always add DataAnnotations on new request DTOs and forward
   `CancellationToken` on async I/O
5. Comment non-obvious features/methods (English, `Example*` style);
   use fluent EF queries in repositories
6. Prefer extending a related existing feature over a new file tree
7. Ask before introducing a new architectural pattern (MediatR, vertical slices, etc.)

