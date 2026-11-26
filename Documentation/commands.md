# Backend commands only

## GIT

```
git pull
```
Fetch the latest changes from the remote repository.

```
git add .
git commit -m "message"
```
Stage then save your changes locally.

```
git push
```
Push your local commits to the remote repository.


## DOCKER (dev database)

`backend.dal/Dockerfile` starts a local PostgreSQL for development.
Do **not** use it in production (password is in the tracked file).

```
cd backend.dal
docker build -t alfred-db .
```
Build the PostgreSQL image from the Dockerfile in this folder.

```
docker run -d --name alfred-db -p 4040:4040 alfred-db
```
Start the container (port `4040`). The Dockerfile `VOLUME` keeps data
across `stop` / `start` of the same container. Recreating it resets the DB.

```
docker stop alfred-db
docker start alfred-db
```
Stop / restart an existing container (data kept).

```
docker logs -f alfred-db
```
Follow PostgreSQL logs.

| Setting | Value |
| --- | --- |
| Host | `localhost` |
| Port | `4040` |
| Database | `alfred_db` |
| Username / Password | see `backend.dal/Dockerfile` |

Connection string: `backend.api/appsettings.Development.json` (`DALConfig`).
The full local string (including password) is tracked on purpose for Docker.


## Local quality (CI parity)

```
dotnet restore backend.sln
dotnet format whitespace backend.sln --no-restore --exclude backend.dal/Migrations
dotnet build backend.sln
dotnet test backend.sln --collect "XPlat Code Coverage"
```

`dotnet format whitespace` may rewrite files — review and commit.
CI uses the same command with `--verify-no-changes`.


## Cloud / App Service config

Same keys as `appsettings` / DI (`DALConfig`, `JwtConfig`, `AppConfig`,
`Cors`). In Azure they become env vars with `__` for nested keys.
Never use `Development` or `Testing` in the cloud.

Replace `APP` and `RG` with your App Service name and resource group.

### Non-secrets

```
az webapp config appsettings set --name APP --resource-group RG --settings ^
  ASPNETCORE_ENVIRONMENT=Production ^
  AppConfig__UseHttpsRedirection=false ^
  Cors__AllowedOrigins__0=https://YOUR_FRONTEND_ORIGIN
```

### Secrets

Where to get them:

| Key | Source |
| --- | --- |
| `DALConfig__ConnectionString` | Azure Postgres (or your DB) portal → connection info; full Npgsql string with password, prefer `Ssl Mode=Require;Trust Server Certificate=false` |
| `JwtConfig__Key` | Generate new (≠ local): `backend.api/genkey.ps1` (≥ 44 Base64 chars) |

```
az webapp config appsettings set --name APP --resource-group RG --settings ^
  DALConfig__ConnectionString="Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=false" ^
  JwtConfig__Key="PASTE_GENKEY_OUTPUT"
```

In the portal, mark those two as secret if the UI offers it (or use Key Vault references).

### Local user-secrets (dev machine only)

```
cd backend.api
dotnet user-secrets set "DALConfig:ConnectionString" "<full-connection-string>"
dotnet user-secrets set "JwtConfig:Key" "<base64-signing-key>"
```

For the local Docker DB, prefer `appsettings.Development.json` instead.


## JWT (user-secrets)

Auth is JWT Bearer. The signing key is **not** in tracked
appsettings — store it in user-secrets (local) or cloud secret
`JwtConfig__Key`. Generate ≥ 32 random bytes (Base64), e.g.
`backend.api/genkey.ps1`. Production key must differ from local.

```
cd backend.api
dotnet user-secrets set "JwtConfig:Key" "<base64-signing-key>"
```

There is no external verification server: put
`Authorization: Bearer <access_token>` on protected requests.
API path base: `/api` (example login: `POST /api/auth/login`).
Register also returns `{ access_token, token_type }`.
Current user: `GET /api/auth/me` (requires Bearer).
Token lifetime: `JwtConfig.ExpirationTime` minutes (see appsettings).
Logout: `POST /api/auth/logout` with the same Bearer token (revokes `jti`).
The token-cleanup daemon starts automatically with
`dotnet run --project backend.api` (no extra command).


## CORS

Allowed origins live under `Cors:AllowedOrigins`.

- **Development:** `appsettings.Development.json` →
  `http://localhost:8080` (Angular). Change the port if the SPA moves
  (e.g. Vite `5173` for a TP).
- **Production:** set exact origins via
  `Cors__AllowedOrigins__0` (no `*`). Adapt when the public frontend
  URL is known.

## DOTNET

```
dotnet restore
```
Download the project's NuGet dependencies (C# package manager).

```
dotnet build
```
Compile the solution to check for errors.

```
dotnet run --project Backend.API
```
Run the API locally on `http://localhost:5050`
(routes under `/api`, Swagger in Development).

```
cd Backend.API
dotnet watch
```
Move into `Backend.API` then run the app in hot reload mode
(reloads on every change without retyping a command).


## TESTING

Project: `Backend.Tests` (xUnit + Moq). Stop a running
`dotnet run` of the API first if the build complains that DLLs
are locked.

```
dotnet test
```
Restore/build as needed, then run **all** unit + integration tests.

```
dotnet test --filter "FullyQualifiedName~Unit"
```
Service unit tests only (fast; Moq, no HTTP host).

```
dotnet test --filter "FullyQualifiedName~Integration"
```
HTTP integration tests only (`WebApplicationFactory` + SQLite).

```
dotnet test --filter "FullyQualifiedName~AuthServiceTests"
```
Run one test class by name (change the filter as needed).

```
dotnet test --logger "console;verbosity=detailed"
```
More output when debugging a failure.

Integration tests do **not** need Docker Postgres: the factory
replaces EF with SQLite and injects a test JWT key.


## MIGRATIONS (Entity Framework)

```
dotnet tool install --global dotnet-ef
```
Install the EF Core tool (once per machine).

```
dotnet ef migrations add MigrationName --project backend.dal --startup-project backend.api
```
Create a new migration from the updated models.

```
dotnet ef database update --project backend.dal --startup-project backend.api
```
Apply pending migrations to the database.

```
dotnet ef migrations remove --project backend.dal --startup-project backend.api
```
Remove the last migration (if not already applied in production).
