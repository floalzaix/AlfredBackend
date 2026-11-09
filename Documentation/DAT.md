# DAT – Alfred Backend

## 1. Context and goals

Alfred is a connected-home manager (SI & Infra project). It aims to make
home-automation ecosystems easier to use and more customizable by
centralizing devices, rooms, and households behind a single API.

This backend exposes the API consumed by the client(s) (responsive web).

## 2. Target functional scope

- Device management via Matter and ZigBee protocols
- Actuator control and sensor display
- 1D scenes for LED synchronization (extensible to 2D/3D)
- Visualization of rooms and the devices they contain
- Authentication and roles: `user`, `home_manager`, `admin`, `admin_reader`
- Household (home) management
- Smart notifications
- Bonus: voice assistant, device simulators

**Expected domain entities** (to confirm): User, Home, Room, Device, Scene.

## 3. Architecture principles

- **N-tier** architecture in 4 projects, with strictly downward dependencies
- Shared domain models across layers (no per-layer duplication)
- Dependencies through **interfaces**, injected (testability)
- Each layer declares its own DI registrations
- Thin routes; business logic only in the BLL
- **REST** resource naming for feature routes

## 4. Application architecture

```
HTTP request
   ↓
Backend.API     Routes → Controllers → DTOs
   ↓
Backend.BLL     Interfaces → Services
   ↓
Backend.DAL     Repositories → AppDbContext
   ↓
Backend.Models  Domain models (properties only)
```

| Project | Role | Contents |
| --- | --- | --- |
| Backend.API | HTTP entry point | Routes, Controllers, DTOs, API DI |
| Backend.BLL | Business logic | Interfaces, Services, BLL DI |
| Backend.DAL | Persistence | Repositories, `AppDbContext`, `DALConfig`, DAL DI |
| Backend.Models | Shared models | POCOs without logic |

**Dependency rules**

- API → BLL, Models
- BLL → DAL, Models
- DAL → Models
- Models → no solution dependencies
- Forbidden: DTO/Controller imported by BLL or DAL, `AppDbContext` from API,
  skipping a layer

DI is **chained**: `Program.cs` → `AddAPI()` → `AddBLL()` → `AddDAL()`.

**Request flow**: Route (binding) → Controller (DTO → service call) →
Service (business rules) → Repository (DB access) → Model.

## 5. Technical stack

| Area | Choice | Status |
| --- | --- | --- |
| Runtime / language | .NET 10, C# 14 | In place |
| API | ASP.NET Core Minimal APIs | In place |
| API docs | Swagger UI (Swashbuckle) + OpenAPI | In place (dev) |
| ORM | Entity Framework Core 10 | Configured |
| Database | PostgreSQL (Npgsql 10.0.0) | Connection strings in appsettings |
| Validation | DataAnnotations on DTOs and config | Partial for DTOs (see §9) |
| Dependency injection | Microsoft.Extensions.DependencyInjection | In place |

## 6. Dependency injection and typed configuration

`Program.cs` only calls `AddAPI()`. Default lifetime: **Scoped**.

| File | Registers |
| --- | --- |
| `Backend.DAL/DependencyInjection.cs` | `DALConfig`, `AppDbContext`, repositories |
| `Backend.BLL/DependencyInjection.cs` | Services (`I*Service` → `*Service`) |
| `Backend.API/DependencyInjection.cs` | Controllers |

### Adding configuration for a project

1. Create `XXXConfig.cs` in the project with DataAnnotations (`[Required]`, …)
2. Add the matching JSON section in `appsettings*.json`
3. Bind and validate in that project's `DependencyInjection.cs`:

```csharp
services.AddOptions<DALConfig>()
    .BindConfiguration("DALConfig")
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

4. Inject `IOptions<XXXConfig>` where values are needed

Reference: `Backend.DAL/DALConfig.cs`.

## 7. Data and database

- `AppDbContext` (`Backend.DAL/Database.cs`) is the entity registry
- To **persist** a model: create it in `Backend.Models`, then declare
  `DbSet<>` (and optional constraints in `OnModelCreating`) in `Database.cs`
- Without that registration, EF will not create/update a table for the model
- No repositories or migrations yet
- EF Core migrations are driven from `Backend.API` (startup) toward `Backend.DAL`
- Connection strings live under `DALConfig` (no secrets hardcoded in code)
- **Dev database**: `Backend.DAL/Dockerfile` (PostgreSQL, port `4040`,
  database `alfred_db`). Commands in `commands.md` / `README.md`.
  Dev-only credentials; do not use that image in production.

| File | Connection string purpose |
| --- | --- |
| `appsettings.json` | Default / shared fallback |
| `appsettings.Development.json` | Local PostgreSQL via Docker (dev) |
| `appsettings.Production.json` | Fake production example |

```json
"DALConfig": {
  "ConnectionString": "Host=…;Port=4040;Database=…;Username=…;Password=…"
}
```

## 8. API interface

- Each feature exposes a route group (`MapGroup("/<feature>")`) as an
  extension method `Map<Feature>Routes()`
- Feature routes must use **REST** names: resource nouns (usually plural)
  + HTTP methods for actions (`GET /devices`, `POST /devices`, …)
  Avoid verb paths (`/getDevices`). Nested resources are fine when ownership
  is clear (`/homes/{id}/rooms`)
- Request DTOs are dedicated and `sealed`; GET DTOs use `[AsParameters]`
- Reference feature: `Example` (demo path `/hello_world` is not REST)

| Method | Route | Description |
| --- | --- | --- |
| GET | `/example/hello_world?name=` | Returns an `ExempleUser` (Id + Name) |

- Local URLs: `http://localhost:5230`, `https://localhost:7247`
- Swagger UI available only in the Development environment

## 9. Security

| Topic | State |
| --- | --- |
| Authentication / role authorization | Planned, not implemented |
| HTTPS | Redirection enabled |
| Secrets | Keep real passwords out of the repo; use secrets / env vars in real prod |
| `AllowedHosts` | `*` (restrict in production) |
| Input validation | DataAnnotations on DTOs; `AddValidation()` not yet called in `Program.cs` |

## 10. Build, run, and deploy

- Tools: `dotnet restore`, `dotnet build`, `dotnet run --project Backend.API`,
  `dotnet watch`
- Dev database: Docker image from `Backend.DAL/Dockerfile`
  (`docker build` / `docker run` — see `commands.md`)
- Editor: VS Code (`launch.json` and `tasks.json` provided)
- API/app containerization, CI/CD, and hosting strategy not defined yet

## 11. Conventions

- PascalCase (types, files, namespaces), camelCase (variables),
  `_camelCase` (private fields)
- `I` prefix for interfaces, `Async` suffix for async methods,
  `Is`/`Has`/`Can` for booleans
- Primary constructors preferred; project content (code, docs, messages)
  in English
- Adding a feature: follow the `Example*` template layer by layer
  (checklist in `AGENTS.md` and `HOW_TO_ADD_A_FEATURE.md`)

## 12. Attention points

1. **Validation inactive**: add `builder.Services.AddValidation()` so
   DataAnnotations are enforced on Minimal APIs.
2. **Path casing**: folders may be lowercase while `Backend.slnx` and project
   references use `Backend.API`. Fine on Windows/macOS; fails on Linux CI.
3. **Persistence**: no entities, repositories, or migrations yet; register
   models in `Database.cs` when adding them.
4. **Controllers**: plain classes injected into routes (not MVC controllers).
5. **Naming**: `ExempleUser` (typo) should align on `Example`.
6. **Swagger and OpenAPI**: both `MapOpenApi()` and Swashbuckle are enabled;
   keep one if possible.
7. **No tests**: no test project in the solution yet.

## 13. Technical roadmap

1. Persistence: entities, repositories, first migration
2. Authentication and roles
3. Core features: Home, Room, Device
4. Matter / ZigBee protocol integration (gateway or adapters)
5. LED scenes and notifications
6. Tests, CI/CD, and API/app containerization (DB Docker already exists for dev)
7. Bonus: voice assistant, simulators
