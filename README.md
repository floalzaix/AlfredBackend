# Projet SI & Infra → Alfred

*Home manager projet*

### Problem: Make home automation ecosystems easier and more customizable (home automation in general)

## Features

- Device management with protocols (Matter, ZigBee...)
- Basic functions for actuators and sensor displays
- 1 dimension scene management system for LED synchronization (potential to upgrade to 2D or 3D)
- Visualization of rooms with devices inside
- Authentication (OAuth2 Bearer JWT) with roles → `User`, `Admin`
  (planned: `home_manager`, `admin_reader`, …)
- Household (home) management system
- Voice assistant (bonus)
- Device simulators to test before buying (bonus)
- Smart notifications
- Responsive design

---

## Technical guide

### Authentication (OAuth2 Bearer JWT)

Auth uses **OAuth2 Bearer JWT**. There is **no external verification
server**: the API issues the token and validates it locally.

1. Register: `POST /auth/register`
2. Login: `POST /auth/login` → `{ "access_token": "<jwt>", "token_type": "Bearer" }`
3. Call protected routes with:
   `Authorization: Bearer <access_token>`

The token expires after `JwtConfig.ExpirationTime` minutes
(default `60`). After expiry, log in again.

Protect a route with `.RequireAuthorization("User")` or
`.RequireAuthorization("Admin")`. JWT settings live under `JwtConfig`
in `appsettings*.json`; the signing `Key` comes from user-secrets /
environment variables. More detail: `Documentation/DAT.md` §9.

### Naming convention

| Kind | Convention | Example |
| --- | --- | --- |
| Types, public members, files, folders, projects, namespaces | PascalCase | `EquipeService`, `MonApp.Bll.Services` |
| Interfaces, async methods, booleans | `I` prefix / `Async` suffix / `Is`·`Has`·`Can` | `IEquipeService`, `ObtenirEquipeAsync()`, `IsActive` |
| Parameters and local variables | camelCase | `equipeId`, `nombreEquipes` |
| Private fields | `_camelCase` | `_equipeRepository` |

### Dev database (Docker)

`Backend.DAL/Dockerfile` runs a **local PostgreSQL** for development
(credentials are intentionally simple and tracked — **dev only**).

```bash
cd Backend.DAL
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
| Username / Password | see `Backend.DAL/Dockerfile` |

Connection string: `Backend.API/appsettings.Development.json` (`DALConfig`).
More commands: `Documentation/commands.md`.

---

## Requirements

- .NET 10 or newer
- C# 14 or newer
- Docker (dev database)
