# AGENTS.md

Instructions for AI agents working on this backend. Follow them for every change.

## Project

Alfred — home automation backend (SI & Infra).

- Stack: .NET 10, C#, Minimal APIs, EF Core, PostgreSQL (planned)
- Architecture: N-tier (layered)
- Reference feature: `Example*` across all layers

All project content (code, comments, docs, commit messages, user-facing strings) must be **English only**.

---

## Architecture (mandatory)

```
HTTP request
    ↓
Backend.API        Routes → Controllers → DTOs
    ↓
Backend.BLL        Interfaces (I*Service) → Services
    ↓
Backend.DAL        Interfaces (I*Repository) → Repositories → AppDbContext
    ↓
Backend.Models     Shared domain models (properties only)
```

### Projects and responsibilities

| Project | Role | May contain |
| --- | --- | --- |
| `Backend.API` | HTTP edge | Routes, Controllers, DTOs, API DI |
| `Backend.BLL` | Business logic | `Interfaces/`, `Services/`, BLL DI |
| `Backend.DAL` | Persistence | `Interfaces/`, `Repositories/`, `AppDbContext`, DAL DI |
| `Backend.Models` | Shared models | POCOs with properties only |

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

1. **Route** (`Backend.API/Routes`) — exposes the HTTP endpoint, binds input, injects the controller
2. **Controller** (`Backend.API/Controllers`) — maps DTO → service call, returns the result
3. **Service** (`Backend.BLL/Services` via interface) — business rules
4. **Repository** (`Backend.DAL/Repositories` via `Backend.DAL/Interfaces`) — database access when needed
5. **Model** (`Backend.Models`) — shared shape of domain data

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

---

## How to add a feature

Use the `Example*` feature as the template. Complete the steps that apply; skip persistence steps only when the feature has no database work.

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
[ ] 8. Service implementation in Backend.BLL/Services
[ ] 9. Register service in Backend.BLL/DependencyInjection.cs
[ ] 10. Request/response DTOs in Backend.API/DTOs
[ ] 11. Controller in Backend.API/Controllers
[ ] 12. Register controller in Backend.API/DependencyInjection.cs
[ ] 13. Routes in Backend.API/Routes + Map* in Program.cs
[ ] 14. Build and smoke-test the endpoints
```

### Step details

#### 1. Models — `Backend.Models/<Feature>Models.cs`

- Properties only (no business logic, no EF attributes unless required later)
- Shared across layers intentionally (avoid duplicating the same type per layer)

#### 2–6. Persistence — `Backend.DAL` (when needed)

- Interface: `I<Feature>Repository` in `Interfaces/` (same pattern as BLL)
- Implementation: `<Feature>Repository` in `Repositories/`
- Register `DbSet<>` and relationships in `AppDbContext` (`Database.cs`)
- Register: `services.AddScoped<I<Feature>Repository, <Feature>Repository>();`
- Migration commands: see `commands.md`

#### 7–9. Business logic — `Backend.BLL`

- Interface: `I<Feature>Service` in `Interfaces/`
- Implementation: `<Feature>Service` in `Services/`, depends on repository interfaces (not concrete types)
- Register: `services.AddScoped<I<Feature>Service, <Feature>Service>();`

#### 10–13. HTTP edge — `Backend.API`

**DTOs** (`DTOs/<Feature>DTOs.cs`):

- Validate input with DataAnnotations (`[Required]`, `[MinLength]`, …)
- Prefer dedicated request/response DTOs; do not expose internal wiring through the API contract when a response DTO is clearer
- Enable validation in the app with `builder.Services.AddValidation();` when using DataAnnotations on Minimal APIs

**Controller** (`Controllers/<Feature>Controller.cs`):

- Inject `I<Feature>Service` (interface, not concrete class)
- Accept DTOs, call the service, return the result
- No direct database access

**DI** (`DependencyInjection.cs`):

- `services.AddScoped<<Feature>Controller>();`

**Routes** (`Routes/<Feature>Route.cs`):

- Static class with `Map<Feature>Routes(this IEndpointRouteBuilder …)`
- Use `MapGroup("/<feature>")`
- Inject the controller from DI; keep handlers thin
- Call `app.Map<Feature>Routes();` from `Program.cs`

**Binding rules (Minimal APIs):**

- Complex query DTOs on `MapGet` → mark with `[AsParameters]` (GET has no body)
- Body DTOs on `MapPost` / `MapPut` / `MapPatch` → body binding is fine
- `[FromQuery]` on a property alone does not make a complex parameter a query model

### What not to do when adding a feature

- Do not put service or repository logic inside route lambdas
- Do not register concrete services where an interface already exists (`IExampleService` pattern)
- Do not add a new project or flatten layers without an explicit architectural decision
- Do not hardcode secrets; connection strings belong in configuration (see DAL DI TODOs)
- Do not leave a new type unregistered in its layer’s `DependencyInjection.cs`

---

## Dependency injection

Each layer owns its registrations and chains the layer below:

| File | Calls next | Registers |
| --- | --- | --- |
| `Backend.API/DependencyInjection.cs` | `AddBLL()` | controllers |
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
// services.AddDbContext<AppDbContext>(...);
// services.AddScoped<IRepository, Repository>();
```

---

## Coding style for this repo

- Prefer primary constructors for Controllers/Services when dependencies are simple
- Prefer interfaces for BLL and DAL collaborators (testability / mocking)
- Prefer `sealed` on DTOs when inheritance is not needed
- Keep routes as extension methods on `IEndpointRouteBuilder`
- Match existing file headers and section comments style when editing nearby code
- Keep user-facing validation messages in English

---

## Commands

See `commands.md` for restore, build, run, watch, and EF migrations.

Minimum verification after a feature:

```bash
dotnet build
dotnet run --project Backend.API
```

---

## When unsure

1. Copy the structure of the closest `Example*` files
2. Respect the dependency table above
3. Prefer a thin route + controller + interface service over shortcuts
4. Ask before introducing a new architectural pattern (MediatR, vertical slices, etc.)
)
