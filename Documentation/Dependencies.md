```mermaid
---
config:
  theme: default
  look: classic
  layout: elk
---
flowchart TB
    subgraph Frontend["Frontend (Angular 22)"]
        Pages["shared/pages"]
        Features["features/*"]
        Core["core<br/>AsyncHttpClient<br/>interceptors / guards"]
    end

    subgraph API["Backend.API"]
        Routes["Routes"]
        Controllers["Controllers"]
        DTOs["DTOs"]
        Cors["CORS Frontend policy"]
        ApiDI["DependencyInjection<br/>AddAPI()"]
        Handler["GlobalExceptionHandler"]
    end

    subgraph BLL["Backend.BLL"]
        ServiceInterfaces["Interfaces<br/>I*Service"]
        Services["Services"]
        BllDI["DependencyInjection<br/>AddBLL()"]
    end

    subgraph DAL["Backend.DAL"]
        RepoInterfaces["Interfaces<br/>I*Repository"]
        Repositories["Repositories"]
        DbContext["AppDbContext"]
        DalDI["DependencyInjection<br/>AddDAL()"]
    end

    subgraph Models["Backend.Models"]
        DomainModels["Domain models"]
        Exceptions["Exceptions<br/>AppException"]
    end

    Program["Program.cs<br/>PathBase /api + JWT + CORS"]

    Pages --> Features
    Features --> Core
    Core -->|"HTTP Bearer + /api"| Cors

    Program -->|"builder.Services.AddAPI()"| ApiDI
    ApiDI -->|"services.AddBLL()"| BllDI
    BllDI -->|"services.AddDAL()"| DalDI
    Program --> Routes
    Program --> Cors
    Program --> Handler

    Routes --> Controllers
    Controllers --> ServiceInterfaces
    Services --> ServiceInterfaces
    Services --> RepoInterfaces
    Services --> Exceptions
    Repositories --> RepoInterfaces
    Repositories --> DbContext
    Handler --> Exceptions

    Controllers --> DomainModels
    Services --> DomainModels
    Repositories --> DomainModels
    DbContext --> DomainModels
    DTOs -.-> Controllers

    API -->|"project ref"| BLL
    API -->|"project ref"| Models
    BLL -->|"project ref"| DAL
    BLL -->|"project ref"| Models
    DAL -->|"project ref"| Models
```
