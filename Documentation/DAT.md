# DAT – Alfred

## 1. Context and goals

Alfred is a connected-home manager (SI & Infra project). It aims to make
home-automation ecosystems easier to use and more customizable by
centralizing devices, rooms, and households behind a single API and a
responsive web client.

- **Backend** (this repository): REST API consumed by the frontend
- **Frontend** (sibling repo `frontend/`): Angular SPA

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

**Implemented today:** User auth (register / login / logout / me), JWT
revocation, Example demo route, Angular auth UI (login / register).

## 3. Architecture principles

- **N-tier** backend in 4 projects, with strictly downward dependencies
- Shared domain models across layers (no per-layer duplication)
- Dependencies through **interfaces**, injected (testability)
- Each layer declares its own DI registrations
- Thin routes; business logic only in the BLL
- **REST** resource naming for feature routes
- **DTO validation**: DataAnnotations on request DTOs with French
  `ErrorMessage` (`AddValidation()` in `Program.cs`)
- **CancellationToken**: propagate request abort through
  Route → Controller → Service → Repository → EF / HTTP (BCL type;
  does not couple BLL/DAL to ASP.NET)
- **Centralized error handling**: expected failures via `AppException`
  subtypes; unexpected failures → 500 ProblemDetails
- Frontend: feature folders + thin routable pages; HTTP via a shared
  client and interceptors

## 4. Backend application architecture

```
HTTP request (/api/…)
   ↓
Backend.API     Routes → Controllers → DTOs
                (+ GlobalExceptionHandler, CORS, JWT, PathBase)
   ↓
Backend.BLL     Interfaces → Services (throw AppException when expected)
   ↓
Backend.DAL     Repositories → AppDbContext
   ↓
Backend.Models  Domain models + Exceptions
```

| Project | Role | Contents |
| --- | --- | --- |
| Backend.API | HTTP entry point | Routes, Controllers, DTOs, `GlobalExceptionHandler`, JWT wiring, CORS, `TokenCleanupBackgroundService`, API DI |
| Backend.BLL | Business logic | Interfaces, Services, BLL DI |
| Backend.DAL | Persistence | Repositories, `AppDbContext`, migrations, `DALConfig`, DAL DI |
| Backend.Models | Shared models | POCOs without logic, `Exceptions/` |

**Dependency rules**

- API → BLL, Models
- BLL → DAL, Models
- DAL → Models
- Models → no solution dependencies
- Forbidden: DTO/Controller imported by BLL or DAL, `AppDbContext` from API,
  skipping a layer

DI is **chained**: `Program.cs` → `AddAPI()` → `AddBLL()` → `AddDAL()`.

**Request flow**: Route (binding + validated DTO + `CancellationToken`)
→ Controller (DTO → service call, forward token) → Service (business
rules; may throw `AppException`; forward token) → Repository (DB access
with token on EF async APIs) → Model. Uncaught exceptions →
`GlobalExceptionHandler` → ProblemDetails.

**Input validation:** request DTOs in `Backend.API/DTOs` must use
DataAnnotations (`[Required]`, `[MinLength]`, `[EmailAddress]`, …)
with French `ErrorMessage`. Do not leave DTOs unannotated. Invalid
input → 400 (`.ProducesValidationProblem()`). Business/DB rules stay
in the BLL as `AppException`.

**Error handling**

| Piece | Role |
| --- | --- |
| `Backend.Models/Exceptions/AppException` (+ subtypes) | Expected errors with HTTP status + title |
| `UnauthorizedException` / `NotFoundException` / `ConflictException` | 401 / 404 / 409 |
| `Backend.API/GlobalExceptionHandler` | `IExceptionHandler`: maps `AppException` or returns generic 500 |
| `AddExceptionHandler` + `AddProblemDetails` + `UseExceptionHandler()` | Pipeline wiring |

Services throw subtypes for expected failures; repositories should
prefer `null` / empty results. Do not throw bare `Exception` for
business cases. Details: `AGENTS.md` (Error handling) and
`HOW_TO_ADD_A_FEATURE.md`.

**User-facing messages (French):** validation errors, `AppException`
messages/titles, and the generic 500 ProblemDetails text shown to
clients are in **French** (frontend display). Code, comments, docs,
and commit messages stay in **English**.

## 5. Backend technical stack

| Area | Choice | Status |
| --- | --- | --- |
| Runtime / language | .NET 10, C# 14 | In place |
| API | ASP.NET Core Minimal APIs | In place |
| API base path | `UsePathBase("/api")` | In place |
| API docs | Swagger UI (Swashbuckle) + OpenAPI | In place (dev) |
| ORM | Entity Framework Core 10 | In place |
| Database | PostgreSQL (Npgsql 10.0.0) | Connection strings in appsettings |
| Migrations | EF Core under `Backend.DAL/Migrations` | Users + UserTokens |
| Validation | DataAnnotations on DTOs and config | In place (`AddValidation`) |
| Auth | JWT Bearer (local validation, no introspect server) | In place |
| CORS | Policy `Frontend` from `Cors:AllowedOrigins` | In place |
| Dependency injection | Microsoft.Extensions.DependencyInjection | In place |

## 6. Dependency injection and typed configuration

`Program.cs` only calls `AddAPI()`. Default lifetime: **Scoped**.

| File | Registers |
| --- | --- |
| `Backend.DAL/DependencyInjection.cs` | `DALConfig`, `AppDbContext`, repositories |
| `Backend.BLL/DependencyInjection.cs` | Services (`I*Service` → `*Service`) |
| `Backend.API/DependencyInjection.cs` | Controllers, exception handler, `TokenCleanupBackgroundService` |

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
- EF Core migrations live in `Backend.DAL/Migrations` and are applied from
  `Backend.API` as startup project
- Connection strings live under `DALConfig` (no secrets hardcoded in code)
- **Dev database**: `Backend.DAL/Dockerfile` (PostgreSQL, port `4040`,
  database `alfred_db`). Commands in `commands.md` / `README.md`.
  Dev-only credentials; do not use that image in production.

### Persisted entities (current)

| Entity | Purpose |
| --- | --- |
| `User` | Account (username, email, password hash, role) |
| `UserToken` | Active JWT `jti` + expiry (logout / revocation) |

Repository: `IAuthRepo` / `AuthRepo` (create user, lookup by email/id,
create/delete/check tokens, cleanup expired tokens).

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

## 8. Backend API interface

- Global path base: **`/api`** (`app.UsePathBase("/api")` in `Program.cs`)
- Effective URLs look like `http://localhost:5050/api/auth/login`
- Each feature exposes a route group (`MapGroup("/<feature>")`) as an
  extension method `Map<Feature>Routes()`
- Feature routes must use **REST** names: resource nouns (usually plural)
  + HTTP methods for actions (`GET /devices`, `POST /devices`, …)
  Avoid verb paths (`/getDevices`). Nested resources are fine when ownership
  is clear (`/homes/{id}/rooms`)
- Request DTOs are dedicated and `sealed`, with **DataAnnotations** and
  French `ErrorMessage` on every constrained field; GET DTOs use
  `[AsParameters]`
- Async handlers bind `CancellationToken` and forward it through
  controller → service → repository → EF
- Document expected statuses with `.Produces` / `.ProducesValidationProblem`
  / `.ProducesProblem` for Swagger
- Reference demo feature: `Example` (path `/example/hello_world` is not REST)
- Reference validated DTOs: `AuthDTOs.cs`, `ExampleDTOs.cs`

| Method | Route (after `/api`) | Auth | Description |
| --- | --- | --- | --- |
| POST | `/auth/register` | Public | Create a user; **returns** `{ access_token, token_type }` |
| POST | `/auth/login` | Public | Validate credentials; return a Bearer JWT |
| POST | `/auth/logout` | `User` policy | Revoke the current JWT (`jti` removed from DB) |
| GET | `/auth/me` | `User` policy | Current user (`id`, `username`, `email`) |
| GET | `/example/hello_world?name=` | `User` policy | Returns an `ExempleUser` (Id + Name) |

- Local API URL: **`http://localhost:5050`** (see `launchSettings.json`)
- Swagger UI available only in the Development environment
  (typically under the `/api` path base)

## 9. Security

### Authentication (JWT Bearer)

Auth is **JWT Bearer**. There is **no external token verification /
introspection server**. The API issues the JWT on login **or register**
and validates it locally on each protected request. Revocation uses
persisted token ids (`jti` in `UserToken`).

**Flow**

1. `POST /api/auth/register` — create an account and receive
   `{ access_token, token_type: "Bearer" }` (same shape as login)
2. `POST /api/auth/login` — get `{ access_token, token_type: "Bearer" }`;
   the JWT `jti` is stored in `UserToken`
3. Call protected routes with header:
   `Authorization: Bearer <access_token>`
4. ASP.NET Core JWT Bearer middleware validates signature, issuer,
   audience, and expiration locally (no remote auth server)
5. Authorization also checks that the `jti` is still active
   (`ActiveJtiHandler`) so logged-out tokens are rejected
6. `GET /api/auth/me` (policy `User`) — return the authenticated user
7. `POST /api/auth/logout` (policy `User`) — deletes the current `jti`
   from the database (token revoked before natural expiry)

**Token**

- Format: JWT (HMAC-SHA256)
- Claims: `sub` (user id), `name`, `role`, `jti` (token id)
- Lifetime: `JwtConfig.ExpirationTime` minutes (default `60`)
- After expiry or logout, the client must log in again to get a new
  token

**Logout and token cleanup daemon**

- Logout removes the matching `UserToken` row for the request JWT
- `TokenCleanupBackgroundService` (`BackgroundService` in
  `Backend.API`) runs with the API process and every **5 minutes**
  calls `IAuthService.CleanupExpiredTokensAsync()` to delete expired
  `UserToken` rows
- The hosted service only schedules work and opens a DI scope; cleanup
  logic stays in the BLL

**Protecting a route**

```csharp
.RequireAuthorization("User")   // roles User or Admin
.RequireAuthorization("Admin")  // role Admin only
```

**Configuration** (`JwtConfig` in `appsettings*.json` + user-secrets /
env for `Key`):

| Setting | Purpose |
| --- | --- |
| `Issuer` | Token issuer (`alfred_backend`) |
| `Audience` | Token audience (`alfred_backend`) |
| `Key` | Base64 signing key (secrets / env — not in repo) |
| `ExpirationTime` | Validity in minutes |

Swagger (dev): use the `bearer` security scheme and paste the JWT
(without the `Bearer` prefix).

### CORS

Policy name: **`Frontend`**. Origins come from configuration:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:8080"
  ]
}
```

- Development: `http://localhost:8080` (Angular `ng serve` port)
- Production placeholder may include `https://<nom hote>`
- Pipeline: `UseCors("Frontend")` before auth middleware
- Allows any header and any method for listed origins
- The SPA calls the API **cross-origin** (no frontend proxy)

| Topic | State |
| --- | --- |
| Authentication / role authorization | Implemented (JWT Bearer + `User` / `Admin` policies) |
| Register returns JWT | Implemented |
| Current user (`GET /auth/me`) | Implemented |
| Logout / JWT revocation (`jti` + `UserToken`) | Implemented |
| Expired-token cleanup daemon | Implemented (`TokenCleanupBackgroundService`) |
| CORS for frontend | Implemented (`Cors:AllowedOrigins`) |
| Global error handling | Implemented (`AppException` + `GlobalExceptionHandler` + ProblemDetails) |
| HTTPS | Redirection enabled |
| Secrets | Keep real passwords and JWT key out of the repo; use secrets / env vars |
| `AllowedHosts` | `*` (restrict in production) |
| Input validation | DataAnnotations on DTOs (French `ErrorMessage`); `AddValidation()` in `Program.cs` |
| Request cancellation | `CancellationToken` propagated to services/repos/EF |

## 10. Frontend technical architecture

Sibling repository: `../frontend` (`alfred-frontend`).

### Stack

| Area | Choice |
| --- | --- |
| Framework | Angular **22** (standalone, `bootstrapApplication`) |
| Language | TypeScript (~ES2022) |
| Build / CLI | Angular CLI / `@angular/build` |
| UI | PrimeNG 22 + Aura theme (`@primeuix/themes`), PrimeIcons |
| Forms | Reactive Forms (Signal Forms preferred for new work) |
| State | Angular **signals** (no NgRx) |
| Routing | `@angular/router` + functional guards |
| HTTP | `@angular/common/http` via custom `AsyncHttpClient` |
| Validation | **Zod** schemas on API DTOs / models |
| Auth storage | JWT in `localStorage` key `access_token` |

### Structure

```
frontend/src/app/
  core/                 Guards, interceptors, shared models, AsyncHttpClient
  features/<name>/      Feature service, DTOs/Zod, feature components
  shared/
    pages/<name>/       Routable page shells (composition only)
    components/         Reusable UI
  app.routes.ts
  app.config.ts
```

**Rules**

- Routes target **`shared/pages/*` only** (never feature components)
- Pages compose feature / shared components; no HTTP in pages
- Features own services, Zod DTOs, and domain UI
- `core/` holds cross-cutting infrastructure

### Backend integration

| Item | Value |
| --- | --- |
| Dev API base URL | `http://localhost:5050/api` (`environment.development.ts`) |
| Prod API base URL | Placeholder in `environment.ts` |
| Auth header | `Authorization: Bearer <token>` via `tokenInterceptor` |
| Dev frontend URL | `http://localhost:8080` (`angular.json` serve port) |

**Endpoints used by the SPA today**

| Method | Endpoint | Purpose |
| --- | --- | --- |
| POST | `/auth/login` | Login; store `access_token` |
| POST | `/auth/register` | Register; store `access_token` |
| GET | `/auth/me` | Current user (`authGuard` / `AuthService.getUser`) |

HTTP errors are mapped to `ApiError` / `ValidationError` and shown with
PrimeNG Toast (French copy). Pipeline: `tokenInterceptor` →
`errorInterceptor`.

### Routes (current)

| Path | Page | Guard | Notes |
| --- | --- | --- | --- |
| `auth` | `AuthPage` | None | Login / register UI |
| `**` | redirect → `auth` | — | Default entry |

`authGuard` exists but is not wired on a private page yet. Logout UI
against `POST /auth/logout` is not implemented in the SPA yet.

### Environments and scripts

- No `.env` files: Angular **file replacements** for development
- Scripts: `npm start` (`ng serve` on **8080**), `npm run build`,
  `npm run watch`, `npm test`
- More detail: frontend `AGENTS.md` and `documentation/HOW_TO_ADD_A_FEATURE.md`

### End-to-end local layout

```
Browser  http://localhost:8080
    ↓  CORS + Bearer JWT
API      http://localhost:5050/api/…
    ↓
PostgreSQL  localhost:4040  (Docker alfred-db)
```

## 11. Build, run, and deploy

**Backend**

- Tools: `dotnet restore`, `dotnet build`,
  `dotnet run --project backend.api`, `dotnet watch`
- Dev database: Docker image from `backend.dal/Dockerfile`
  (`docker build` / `docker run` — see `commands.md`)
- Editor: VS Code (`launch.json` and `tasks.json` provided)

**Frontend**

- `npm install` then `npm start` (port **8080**)
- Requires API + CORS origin `http://localhost:8080`

API/app containerization, CI/CD, and hosting strategy not defined yet.

## 12. Conventions

- PascalCase (types, files, namespaces), camelCase (variables),
  `_camelCase` (private fields)
- `I` prefix for interfaces, `Async` suffix for async methods,
  `Is`/`Has`/`Can` for booleans
- Primary constructors preferred
- Request DTOs: DataAnnotations + French `ErrorMessage` (mandatory)
- Async I/O: propagate `CancellationToken` (route → … → repository/EF)
- Comment non-obvious features/methods (English, `Example*` file-header style)
- Repository queries: EF Core fluent chains (`Where` / `OrderBy` /
  `Select` / `To*Async`), not raw SQL or LINQ query syntax
- Project content (code, comments, docs, commits) in **English**
- **Exception:** user-facing error / validation messages in **French**
- Adding a feature: follow the `Example*` / `Auth*` template layer by layer
  (checklist in `AGENTS.md` and `HOW_TO_ADD_A_FEATURE.md`)
- Closely related capabilities stay in the same feature file set
  (e.g. home invitation codes → extend `Home*`, do not create
  `Invitation*` everywhere)

## 13. Attention points

1. **Path casing**: solution / csproj paths use lowercase
   (`backend.api`, …). Fine on Windows/macOS; keep consistent for Linux CI.
2. **Path base**: all API URLs include `/api`; do not omit it from clients.
3. **Ports**: API `5050`, frontend `8080`, PostgreSQL `4040` — keep CORS
   and `environment.development.ts` aligned.
4. **Controllers**: plain classes injected into routes (not MVC controllers).
5. **Naming**: `ExempleUser` (typo) should align on `Example`.
6. **Swagger and OpenAPI**: both `MapOpenApi()` and Swashbuckle are enabled;
   keep one if possible.
7. **No automated tests** yet (backend or frontend).
8. **Frontend gaps**: no post-auth pages, logout UI, or `authGuard` on
   private routes yet.

## 14. Technical roadmap

1. Frontend: protected pages, logout, wire `authGuard`
2. Extend roles beyond current `User` / `Admin` policies
   (`home_manager`, `admin_reader`, …)
3. Core features: Home, Room, Device (API + SPA)
4. Matter / ZigBee protocol integration (gateway or adapters)
5. LED scenes and notifications
6. Tests, CI/CD, and API/app containerization (DB Docker already exists for dev)
7. Bonus: voice assistant, simulators
