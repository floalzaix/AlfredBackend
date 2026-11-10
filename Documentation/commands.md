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

`Backend.DAL/Dockerfile` starts a local PostgreSQL for development.
Do **not** use it in production (password is in the tracked file).

```
cd Backend.DAL
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
| Username / Password | see `Backend.DAL/Dockerfile` |

Connection string: `Backend.API/appsettings.Development.json` (`DALConfig`).


## JWT (user-secrets)

Auth is OAuth2 Bearer JWT. The signing key is **not** in tracked
appsettings — store it in user-secrets (or an env var) for local dev.

```
cd Backend.API
dotnet user-secrets set "JwtConfig:Key" "<base64-signing-key>"
```
Set the JWT signing key used by token generation and local validation.
There is no external verification server: put
`Authorization: Bearer <access_token>` on protected requests.
Token lifetime: `JwtConfig.ExpirationTime` minutes (see appsettings).


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
Run the API locally.

```
cd Backend.API
dotnet watch
```
Move into `Backend.API` then run the app in hot reload mode
(reloads on every change without retyping a command).


## MIGRATIONS (Entity Framework)

```
dotnet tool install --global dotnet-ef
```
Install the EF Core tool (once per machine).

```
dotnet ef migrations add MigrationName --project Backend.DAL --startup-project Backend.API
```
Create a new migration from the updated models.

```
dotnet ef database update --project Backend.DAL --startup-project Backend.API
```
Apply pending migrations to the database.

```
dotnet ef migrations remove --project Backend.DAL --startup-project Backend.API
```
Remove the last migration (if not already applied in production).
