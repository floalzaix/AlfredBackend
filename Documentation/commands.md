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
Dev puts `DBPassword` in that file (Docker password is tracked on purpose).


## Database password (prod)

`DALConfig.ConnectionString` keeps a placeholder
`Password={PASSWORD_PLACEHOLDER}`. At startup,
`GetFullConnectionString()` replaces it with `DALConfig.DBPassword`.

**Production:** never commit the real password. Inject it as an
environment variable (ASP.NET maps `__` to `:`):

```
DALConfig__DBPassword=<azure-postgres-password>
```

Set that on the host / App Service / container before starting the API.
Missing `DBPassword` fails startup (`ValidateOnStart`).

**Local optional alternative** (same store as JWT, once per machine —
not per terminal):

```
cd backend.api
dotnet user-secrets set "DALConfig:DBPassword" "<password>"
```

User-secrets are stored under `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\`
and are loaded automatically in Development. Prefer
`appsettings.Development.json` for the local Docker DB password.


## JWT (user-secrets)

Auth is JWT Bearer. The signing key is **not** in tracked
appsettings — store it in user-secrets (or an env var) for local dev.

```
cd backend.api
dotnet user-secrets set "JwtConfig:Key" "<base64-signing-key>"
```
Set the JWT signing key used by token generation and local validation.
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

Allowed origins live under `Cors:AllowedOrigins` in
`backend.api/appsettings*.json`. Development expects the Angular
app at `http://localhost:8080`. Update that list if the SPA port
changes.


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
dotnet run --project backend.api
```
Run the API locally on `http://localhost:5050`
(routes under `/api`, Swagger in Development).

```
cd backend.api
dotnet watch
```
Move into `backend.api` then run the app in hot reload mode
(reloads on every change without retyping a command).


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
