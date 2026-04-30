# TodoApi (Phase 2)

Developer summary

Overview
- ASP.NET Core Web API with controllers and Entity Framework Core using SQLite as the data store.
- Provides a small REST API for managing Todo items and includes OpenAPI/Swagger for interactive testing in development.

What it does
- Exposes CRUD endpoints at `/api/todos` (see `Controllers/TodosController.cs`).
- Uses `AppDbContext` (EF Core) and simple DTOs for the public contract.

Architecture 
- `Program.cs` wires DI and registers `AppDbContext` (SQLite) and controllers.
- `Data/AppDbContext.cs` — EF Core DbContext that exposes `DbSet<TodoItem>`.
- `Controllers/TodosController.cs` — standard controller-based API with routes for GET/POST/PUT/DELETE.
- `Models/` contains both the EF entity (`TodoItem`) and the DTOs used by the API.

Key files
- `Program.cs` — host and service registration.
- `Data/AppDbContext.cs` — EF Core context.
- `Controllers/TodosController.cs` — API endpoints.
- `Models/TodoItem.cs` and `Models/TodoItemDto.cs` — DB entity and request/response contracts.
- `appsettings.json` — connection string (`Data Source=todos.db`).

How to run (developer)
Make sure the .NET SDK matching the project (`TargetFramework` = `net10.0`) is installed. From this folder:

```bash
cd phase-2/TodoApi
dotnet restore
dotnet build

# (optional) install EF CLI if you plan to run migrations
dotnet tool install --global dotnet-ef

# If migrations exist, apply them. Otherwise create a migration then apply.
dotnet ef database update

dotnet run
```

Notes
- The default connection string targets `todos.db` in the project folder (see `appsettings.json`).
- In development the project enables Swagger UI — visit the URL shown in the console (common defaults are `http://localhost:5164` and `https://localhost:7127`).
- If you need to create a migration from scratch: `dotnet ef migrations add InitialCreate` then `dotnet ef database update`.

Testing
- There's a lightweight `TodoApi.http` file included with an example request (you can use it with the REST client extension or curl/Postman).

