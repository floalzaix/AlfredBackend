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

---

## Requirements

- .NET 10 or newer
- C# 14 or newer
- Docker (dev database)
