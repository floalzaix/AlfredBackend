# Projet SI & Infra → Alfred

*Home manager projet*

### Problem: Make home automation ecosystems easier and more customizable (home automation in general)

## Features

- Device management with protocols (Matter, ZigBee...)
- Basic functions for actuators and sensor displays
- 1 dimension scene management system for LED synchronization (potential to upgrade to 2D or 3D)
- Visualization of rooms with devices inside
- Authentication (JWT Bearer) with roles → `User`, `Admin`
  (planned: `home_manager`, `admin_reader`, …)
- Household (home) management system
- Voice assistant (bonus)
- Device simulators to test before buying (bonus)
- Smart notifications
- Responsive design (Angular frontend)

---

## Technical guide

### Repositories

| Repo | Role |
| --- | --- |
| This repo (`backend`) | .NET 10 REST API (`/api`) |
| Sibling `frontend/` | Angular 22 SPA (PrimeNG, Zod) |

Full architecture (backend + frontend): `Documentation/DAT.md`.

### Local ports

| Service | URL / port |
| --- | --- |
| API | `http://localhost:5050` (path base `/api`) |
| Frontend | `http://localhost:8080` |
| PostgreSQL (Docker) | `localhost:4040` |

### Authentication (JWT Bearer)

Auth uses **JWT Bearer** tokens. There is **no external verification
server**: the API issues the token and validates it locally. Active
tokens are also tracked in the database (`UserToken` / JWT `jti`) so
logout can revoke them before expiry.

1. Register: `POST /api/auth/register` →
   `{ "access_token": "<jwt>", "token_type": "Bearer" }`
2. Login: `POST /api/auth/login` → same token response
3. Call protected routes with:
   `Authorization: Bearer <access_token>`
4. Current user: `GET /api/auth/me` (requires `User`)
5. Logout: `POST /api/auth/logout` (requires `User`) — revokes the
   current token by removing its `jti` from the database

The token expires after `JwtConfig.ExpirationTime` minutes
(default `60`). After expiry or logout, log in again.

A background daemon (`TokenCleanupBackgroundService`) runs with the
API and periodically deletes expired `UserToken` rows (every 5 minutes).

Protect a route with `.RequireAuthorization("User")` or
`.RequireAuthorization("Admin")`. JWT settings live under `JwtConfig`
in `appsettings*.json`; the signing `Key` comes from user-secrets /
`JwtConfig__Key` (≥ 32 bytes Base64; prod key ≠ local). Cloud env
checklist: `Documentation/commands.md`.

### CORS

Policy `Frontend` reads `Cors:AllowedOrigins`. Development allows
`http://localhost:8080` so the Angular app can call the API
cross-origin. Production: set exact origins with
`Cors__AllowedOrigins__0` (no wildcards).

### HTTPS

`AppConfig:UseHttpsRedirection` — `false` on Azure App Service when
HTTPS Only terminates TLS at the proxy (do not disable HTTPS Only).

### Errors

Expected failures use `AppException` subtypes → ProblemDetails via
`GlobalExceptionHandler`. **User-facing** validation and error
messages are in **French**; code and docs stay in English.

### Input validation and cancellation

- Request DTOs use **DataAnnotations** with French `ErrorMessage`
  (`AddValidation()` in `Program.cs`) — see `AuthDTOs.cs`
- Async I/O propagates **`CancellationToken`** from the route through
  controller → service → repository → EF

Details: `Documentation/AGENTS.md`.

### Naming convention

| Kind | Convention | Example |
| --- | --- | --- |
| Types, public members, files, folders, projects, namespaces | PascalCase | `DeviceService`, `Backend.BLL.Services` |
| Interfaces, async methods, booleans | `I` prefix / `Async` suffix / `Is`·`Has`·`Can` | `IDeviceService`, `GetDeviceAsync()`, `IsActive` |
| Parameters and local variables | camelCase | `deviceId` |
| Private fields | `_camelCase` | `_deviceRepository` |

### Dev database (Docker)

`backend.dal/Dockerfile` runs a **local PostgreSQL** for development
(credentials are intentionally simple and tracked — **dev only**).

```bash
cd backend.dal
docker build -t alfred-db .
docker run -d --name alfred-db -p 4040:4040 alfred-db
```

Data persists across `stop` / `start` of the same container via the
`VOLUME` in the Dockerfile. Recreating the container starts a fresh DB.

| Setting | Value |
| --- | --- |
| Host | `localhost` |
| Port | `4040` |
| Database | `alfred_db` |
| Username / Password | see `backend.dal/Dockerfile` |

Connection string: `backend.api/appsettings.Development.json` (`DALConfig`).
Persisted entities today: `User`, `UserToken` (EF migrations in
`backend.dal/Migrations`). More commands: `Documentation/commands.md`.

### Run the API

```bash
dotnet restore
dotnet build
dotnet run --project backend.api
```

Swagger (Development): open the app on port `5050` under `/api`.

---

## Requirements

- .NET 10 or newer
- C# 14 or newer
- Docker (dev database)
- For the SPA: Node.js / npm (see `frontend/README.md`)

## Documentation

| Document | Purpose |
| --- | --- |
| `Documentation/DAT.md` | Technical architecture (backend + frontend) |
| `Documentation/AGENTS.md` | Conventions for AI / contributors |
| `Documentation/HOW_TO_ADD_A_FEATURE.md` | Step-by-step feature guide |
| `Documentation/commands.md` | Git, Docker, JWT secrets, dotnet, EF |
| `Documentation/Dependencies.md` | Layer dependency diagram |
| `Documentation/TO_LEARN.md` | Learning checklist |
