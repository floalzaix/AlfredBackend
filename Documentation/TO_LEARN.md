- Review OOP basics
- Compare C# vs Python: primitive types, OOP syntax, and complex types
- Structure and definition of a solution in C#
- Structure and definition of a project in C#
- Common commands and their use
- C# specific keywords
- Understand what "business logic" means in software development (domain/business logic)
- Look at the code and use shift-click to explore what you don't understand
- Understand dependency injection and why it is useful
- Definition of database migrations
- Understand how database integration works in an application
- Run the local PostgreSQL with `Backend.DAL/Dockerfile` (see `commands.md`)
- JWT Bearer auth: register/login → `access_token` → `Authorization`
  header; path base `/api`; local validation (no introspection server);
  token expiry (`JwtConfig.ExpirationTime`); policies `User` / `Admin`;
  `GET /api/auth/me`; logout (`POST /api/auth/logout`) revokes the JWT
  `jti` in `UserToken`; `TokenCleanupBackgroundService` daemon cleans
  expired tokens (see `DAT.md` §9)
- CORS: why browsers need `Cors:AllowedOrigins` for
  `http://localhost:8080` → `http://localhost:5050`
- French user-facing error messages vs English code/docs
- Frontend overview (Angular 22, features/pages, interceptors) —
  see `DAT.md` §10
- C# annotations
- Read example files and try implementing something yourself (without committing) to see if it works. ("Blank page syndrome" is normal—if blocked, ask the AI for help to get an overview.)
    - To check if it works, open http://localhost:5050 (Swagger under `/api`)
      to see if the endpoint appears, then test it and see what it returns.
    - For protected routes: login via `/api/auth/login`, authorize with the
      Bearer JWT in Swagger, then call the endpoint.
- Only comment code that is not obvious.
    - File header comment is mandatory.
    - Function (doxygen) header comment is not needed if purpose is obvious.
    - Block comments (///) are helpful for visual separation.