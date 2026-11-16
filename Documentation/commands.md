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
Token lifetime: `JwtConfig.ExpirationTime` minutes (see appsettings).
Logout: `POST /auth/logout` with the same Bearer token (revokes `jti`).
The token-cleanup daemon starts automatically with
`dotnet run --project backend.api` (no extra command).


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
Run the API locally.

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
