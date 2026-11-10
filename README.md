# Projet SI & Infra → Alfred

*Home manager projet*

### Problem: Make home automation ecosystems easier and more customizable (home automation in general)

## Features

- Device management with protocols (Matter, ZigBee...)
- Basic functions for actuators and sensor displays
- 1 dimension scene management system for LED synchronization (potential to upgrade to 2D or 3D)
- Visualization of rooms with devices inside
- Authentication system with roles → user, home_manager, admin, admin_reader
- Household (home) management system
- Voice assistant (bonus)
- Device simulators to test before buying (bonus)
- Smart notifications
- Responsive design

---

## Technical guide

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

### Households and invitation codes

After starting PostgreSQL, apply the migrations before running the API:

```bash
dotnet ef database update --project backend.dal --startup-project backend.api
dotnet run --project backend.api
```

Create a household with `POST /households`:

```json
{ "name": "My home", "userId": "alice" }
```

The `201` response contains the household's `id`, `name`, and `invitationCode`.
The creator is automatically added as a member. Share the code with another
person, who can join with `POST /households/join`:

```json
{ "code": "ABCDEFGH23456789", "userId": "bob" }
```

Use the actual invitation code from the creation response; codes are
case-insensitive. Joining returns `200`, an unknown code returns `404`, and an
existing membership returns `409`. Names are limited to 100 characters and
user identifiers to 128 characters.

Authentication is not implemented yet: `userId` is currently supplied by the
caller and is not verified. These endpoints must be connected to authenticated
identities before they are exposed to untrusted clients.

Run the household service checks with `dotnet run --project backend.tests`.

---

## Requirements

- .NET 10 or newer
- C# 14 or newer
- Docker (dev database)
