# ***Embrace IT – TODO List API***

## ***Overview***
This project is a **TODO List application** developed as an internship exercise at *Embrace IT*.  
It consists of a **.NET 9 Minimal API backend** with **Clean Architecture principles** and an **Angular frontend**.  
The goal is to provide a scalable, secure, and well‑structured application with authentication, task management, and API documentation.

## ***Tech Stack***
- **Backend**: .NET 10 Minimal API, Entity Framework Core, Identity, JWT
- **Frontend**: Angular 21 (modular architecture: Core, Auth, Features, Shared) -> https://github.com/krauserrobledo/TODO-List-APP-frontend
- **Database**: SQL Server
- **Documentation**: Swagger / Swashbuckle

## ***Project Structure***
```
Solution
 ├── Domain        → Entities, repository contracts
 ├── Application   → Service interfaces, DTOs, business logic
 ├── Data          → DbContext, repositories, EF Core configurations
 └── MinimalApi    → Endpoints, Middlewares, DI setup
```

## ***Features***
- **Authentication & Authorization** with JWT + Identity
- **CRUD operations** for:
  - Tasks
  - Subtasks
  - Categories
  - Tags
- **Middleware** for global exception handling and request logging
- **Swagger documentation** with annotated endpoints
- **Clean Architecture** with clear dependency flow:
  - Domain → Application → Data → Minimal API

## ***Required Backend Technologies***
The backend requires the following technologies and implementations:

- **Core Frameworks**
  - .NET 10 (Updated Latest LTS)
  - Minimal API (Controller‑Based)  
  - Layered Architecture  

- **API Implementations**
  - Identity (user management & authentication)  
  - Middlewares (exception handling, logging)  
  - LINQ queries (method syntax)  
  - Repository pattern  
  - Entity Framework Core  
  - Dependency Injection  
  - External Entities Configuration (DbContext mappings)  
  - JWT (authorization & authentication)  

## ***Setup & Installation***

1. Clone the repository
2. Configure connection string in `appsettings.json`
3. Apply migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the API:
   ```bash
   dotnet run
   ```

## ***API Documentation***
Once the backend is running, access Swagger UI at:
```
http://localhost:5000/swagger
```

## ***Testing & Debugging***
- Endpoints tested via **Swagger**
- Common error handling:
  - Fixed connection string issues (HTTP 500)
  - Added missing `UserName` in DTOs (HTTP 400)
  - Corrected inverted conditions in services (HTTP 401/500)
  - Adjusted LINQ queries for case handling (HTTP 409 → 400)
  - Fixed parameter mismatches between endpoints and services

## ***Git Workflow***
- **Conventional Commits and Branching** adopted for clarity
- Branches organized as:
  - `feature/` → new features
  - `bugfix/` → error fixes
  - `chore/` → maintenance tasks
- Pull requests based on `develop` branch with conflict resolution
