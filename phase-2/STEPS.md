# Phase 2.9 — Todo REST API (EF Core + SQLite)

> Goal: Build a fully working REST API for a Todo app using **ASP.NET Core Web API**, **Entity Framework Core**, and **SQLite**.  
> Endpoints: `GET /api/todos`, `POST /api/todos`, `PUT /api/todos/{id}`, `DELETE /api/todos/{id}`.  
> Analogies from your Flutter/Flask background are included throughout.

---

## What You're Building

A JSON REST API that persists Todo items to a SQLite database — 

| Layer | Responsibility | File(s) |
|---|---|---|
| **Model** | Domain entity (maps to a DB table) | `Models/TodoItem.cs` |
| **DbContext** | EF Core database access — like SQLAlchemy's `db.session` | `Data/AppDbContext.cs` |
| **DTO** | Input/output shapes — separate from the DB entity | `Models/TodoItemDto.cs` |
| **Controller** | Route handlers — like Flask blueprints | `Controllers/TodosController.cs` |
| **Program.cs** | App bootstrap — register services, configure middleware | `Program.cs` |

> **Flask analogy**: `TodoItem` = your SQLAlchemy `Model`, `AppDbContext` = `db` session, `TodosController` = a Blueprint with `@app.route(...)` decorators, `Program.cs` = `app = Flask(__name__)` + `app.config`.

No frontend. Test everything with the built-in **Swagger UI** that ASP.NET generates automatically.

---

## [x] Step 1 — Scaffold the Project

```bash
cd phase-2/2.9-todo-api
dotnet new webapi -n TodoApi --use-controllers
cd TodoApi
```

Open the folder in VS Code:
```bash
code .
```

You'll see the generated structure. Clean up the boilerplate — **delete** the auto-generated example files:
```bash
rm Controllers/WeatherForecastController.cs
rm WeatherForecast.cs
```

By the end you'll have:
```
TodoApi/
├── Controllers/
│   └── TodosController.cs      # Route handlers — like Flask blueprints
├── Data/
│   └── AppDbContext.cs         # EF Core DB session — like SQLAlchemy db
├── Models/
│   ├── TodoItem.cs             # DB entity
│   └── TodoItemDto.cs          # Input/output DTO shapes
├── Program.cs                  # App bootstrap
├── appsettings.json            # Config — like .env / config.py
└── TodoApi.csproj
```

---

## [x] Step 2 — Install EF Core Packages

EF Core is not bundled with the webapi template. Add the SQLite provider and the design tools (needed for migrations):

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

> **Flask analogy**: this is `pip install flask-sqlalchemy alembic` — the ORM and its migration tooling.

Verify they appear in `TodoApi.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="..." />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="..." />
</ItemGroup>
```

---

## [x] Step 3 — Define the `TodoItem` Entity

Create the `Models/` folder and add the domain entity. This is the class EF Core will map to a SQLite table.

```bash
mkdir Models
```

```csharp
// Models/TodoItem.cs
// This is the DB entity — EF Core reads this class to create the "TodoItems" table.
// Flask analogy: class TodoItem(db.Model): id = db.Column(db.Integer, ...)
namespace TodoApi.Models;

public class TodoItem
{
    public int Id { get; set; }                            // Primary key — EF Core convention: property named "Id" → PK
    public string Title { get; set; } = string.Empty;     // NOT NULL column
    public bool IsComplete { get; set; }                   // bool → INTEGER (0/1) in SQLite
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Why a separate DTO below?**  
Exposing the DB entity directly in your API leaks DB concerns (like `Id` on create, or `CreatedAt` the client shouldn't control). DTOs are the public contract your API promises — the entity is an internal implementation detail.

---

## [x] Step 4 — Define the DTO Shapes

Add `Models/TodoItemDto.cs`. We use C# `record` types — they're immutable and auto-generate equality, perfect for request/response shapes.

```csharp
// Models/TodoItemDto.cs
// DTOs (Data Transfer Objects) define the API's public contract.
// They're separate from the DB entity so you can change DB schema without breaking clients.
// Flask analogy: Pydantic models / Marshmallow schemas
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

// Response DTO — what the API sends back (includes Id and CreatedAt)
public record TodoItemResponse(int Id, string Title, bool IsComplete, DateTime CreatedAt);

// Create DTO — what the client sends to POST (no Id — DB generates it)
public record CreateTodoRequest(
    [Required][MaxLength(200)] string Title
);

// Update DTO — what the client sends to PUT (can update title and/or completion)
public record UpdateTodoRequest(
    [MaxLength(200)] string? Title,    // null = don't change
    bool? IsComplete                   // null = don't change
);
```

> **[Required]** and **[MaxLength]** are Data Annotations — ASP.NET validates these automatically before your action method runs (because `[ApiController]` is on the controller). Same idea as Pydantic validators in Flask.

---

## [x] Step 5 — Create the `AppDbContext`

The DbContext is EF Core's unit-of-work — it tracks entity changes and translates LINQ into SQL.

```bash
mkdir Data
```

```csharp
// Data/AppDbContext.cs
// This is the database session class.
// Flask/SQLAlchemy analogy: db = SQLAlchemy(app) — the object you call db.session.query() on.
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSet<T> = a table you can query via LINQ
    // Flask analogy: TodoItem.query.all() → _context.TodoItems.ToListAsync()
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}
```

**Primary constructor** (`(DbContextOptions<AppDbContext> options) : DbContext(options)`) is C# 12 syntax — it passes the options (connection string, provider) to the base `DbContext`. This is wired up in `Program.cs` next.

---

## [x] Step 6 — Configure `Program.cs`

This is the app bootstrap — register the DbContext and the EF Core SQLite provider.

Replace the contents of `Program.cs` with:

```csharp
// Program.cs — App bootstrap
// Flask analogy: app = Flask(__name__) + app.config + db.init_app(app)
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Register services (DI container) ---

// DbContext — tell EF Core to use SQLite with the connection string from appsettings.json
// Flask analogy: app.config["SQLALCHEMY_DATABASE_URI"] = "sqlite:///app.db"
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Controllers — scans for classes that inherit ControllerBase and maps their routes
builder.Services.AddControllers();

// OpenAPI / Swagger — auto-generates interactive API docs (no setup needed)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Configure the HTTP request pipeline (middleware) ---

if (app.Environment.IsDevelopment())
{
    // Swagger UI at /swagger — like Postman but built-in, zero config
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();   // wire up all [Route] / [HttpGet] / [HttpPost] attributes

app.Run();
```

Now add the connection string to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=todos.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> `Data Source=todos.db` tells SQLite to create/use a file called `todos.db` in the project's working directory (the same folder as the `.csproj`). This is the SQLite equivalent of Flask's `SQLALCHEMY_DATABASE_URI = "sqlite:///app.db"`.

---

## [!] Step 7 — Create Migrations and the Database

EF Core's migration system is the .NET equivalent of Alembic (Flask-Migrate). First install the CLI tools if you haven't already:

```bash
dotnet tool install --global dotnet-ef
```

Then generate the first migration (a C# snapshot of the current schema):

```bash
dotnet ef migrations add InitialCreate
```

This creates a `Migrations/` folder with two files:
- `<timestamp>_InitialCreate.cs` — the Up/Down migration (like an Alembic revision)
- `AppDbContextModelSnapshot.cs` — EF Core's snapshot of the current model

Apply the migration to create the `todos.db` SQLite file:

```bash
dotnet ef database update
```

You'll now have a `todos.db` file in the project directory. You can inspect it with any SQLite viewer (e.g. the **SQLite Viewer** VS Code extension).

> **Flask-Migrate analogy**:
> ```bash
> flask db migrate -m "InitialCreate"   →   dotnet ef migrations add InitialCreate
> flask db upgrade                       →   dotnet ef database update
> ```

---

## [x] Step 8 — Build the `TodosController`

This is the core of the API. Create `Controllers/TodosController.cs`:

```csharp
// Controllers/TodosController.cs
// Flask analogy: a Blueprint with @app.route decorators
// Each method = one Flask route function
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]                 // enables automatic 400 validation responses, model binding
[Route("api/[controller]")]     // [controller] → "todos" (class name minus "Controller")
public class TodosController(AppDbContext db) : ControllerBase
{
    // ---------------------------------------------------------------
    // GET /api/todos — list all todos
    // Flask: @bp.route("/todos", methods=["GET"])
    // ---------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemResponse>>> GetAll()
    {
        var todos = await db.TodoItems
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoItemResponse(t.Id, t.Title, t.IsComplete, t.CreatedAt))
            .ToListAsync();

        return Ok(todos);
    }

    // ---------------------------------------------------------------
    // GET /api/todos/{id} — get a single todo
    // Flask: @bp.route("/todos/<int:id>", methods=["GET"])
    // ---------------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemResponse>> GetById(int id)
    {
        var todo = await db.TodoItems.FindAsync(id);

        if (todo is null)
            return NotFound();  // 404 — like Flask's abort(404)

        return Ok(new TodoItemResponse(todo.Id, todo.Title, todo.IsComplete, todo.CreatedAt));
    }

    // ---------------------------------------------------------------
    // POST /api/todos — create a new todo
    // Flask: @bp.route("/todos", methods=["POST"])
    // ---------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<TodoItemResponse>> Create(CreateTodoRequest request)
    {
        // [ApiController] already validated [Required] / [MaxLength] before we get here
        var todo = new TodoItem
        {
            Title = request.Title
            // IsComplete defaults to false, CreatedAt defaults to UtcNow
        };

        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();    // like db.session.commit() in Flask

        var response = new TodoItemResponse(todo.Id, todo.Title, todo.IsComplete, todo.CreatedAt);

        // 201 Created with a Location header pointing to GET /api/todos/{id}
        // Flask equivalent: return jsonify(response), 201, {"Location": f"/api/todos/{todo.id}"}
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, response);
    }

    // ---------------------------------------------------------------
    // PUT /api/todos/{id} — update title and/or completion status
    // Flask: @bp.route("/todos/<int:id>", methods=["PUT"])
    // ---------------------------------------------------------------
    [HttpPut("{id}")]
    public async Task<ActionResult<TodoItemResponse>> Update(int id, UpdateTodoRequest request)
    {
        var todo = await db.TodoItems.FindAsync(id);

        if (todo is null)
            return NotFound();

        // Only update fields that the client actually sent (non-null)
        if (request.Title is not null)
            todo.Title = request.Title;

        if (request.IsComplete is not null)
            todo.IsComplete = request.IsComplete.Value;

        await db.SaveChangesAsync();

        return Ok(new TodoItemResponse(todo.Id, todo.Title, todo.IsComplete, todo.CreatedAt));
    }

    // ---------------------------------------------------------------
    // DELETE /api/todos/{id} — delete a todo
    // Flask: @bp.route("/todos/<int:id>", methods=["DELETE"])
    // ---------------------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await db.TodoItems.FindAsync(id);

        if (todo is null)
            return NotFound();

        db.TodoItems.Remove(todo);
        await db.SaveChangesAsync();

        return NoContent();  // 204 — the standard success response for DELETE
    }
}
```

**Key patterns to note:**

| Pattern | What it does | Flask equivalent |
|---|---|---|
| `[ApiController]` | Auto-validates DTOs, returns 400 on invalid input | `@app.errorhandler(400)` + Pydantic |
| `[Route("api/[controller]")]` | `[controller]` resolves to `"todos"` at runtime | `Blueprint("todos", url_prefix="/api/todos")` |
| `ActionResult<T>` | Can return a typed `T` or any HTTP result (404, 204…) | `return jsonify(...)` or `abort(...)` |
| `FindAsync(id)` | Looks up by PK; returns `null` if not found | `db.session.get(TodoItem, id)` |
| `SaveChangesAsync()` | Flushes all pending changes to the DB | `db.session.commit()` |
| `CreatedAtAction(...)` | Returns 201 + `Location` header | `return jsonify(...), 201, {"Location": ...}` |
| `NoContent()` | Returns 204 No Content | `return "", 204` |

---

## [x] Step 9 — Run and Test with Swagger

```bash
dotnet run
```

ASP.NET will print something like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7234
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5234
```

Open **Swagger UI** in your browser:
```
https://localhost:7234/swagger
```

You'll see all four endpoints auto-documented. Test them in order:

1. **POST /api/todos** — create a todo:
   ```json
   { "title": "Buy groceries" }
   ```
   Expected: `201 Created` with a response body containing the new `id`.

2. **GET /api/todos** — list all todos:  
   Expected: `200 OK` with an array containing your new todo.

3. **GET /api/todos/{id}** — get the todo you just created (use its `id`):  
   Expected: `200 OK` with the single item.

4. **PUT /api/todos/{id}** — mark it complete:
   ```json
   { "isComplete": true }
   ```
   Expected: `200 OK` with `"isComplete": true`.

5. **DELETE /api/todos/{id}** — delete it:  
   Expected: `204 No Content`.

6. **GET /api/todos/{id}** — try to fetch the deleted item:  
   Expected: `404 Not Found`.

7. **POST /api/todos** with an empty title — test validation:
   ```json
   { "title": "" }
   ```
   Expected: `400 Bad Request` with a validation error (handled automatically by `[ApiController]`).

---

## Step 10 — Stretch Goals (optional)

### 10.1 Add a `?complete=true` filter to `GET /api/todos`

```csharp
// Replace GetAll() with:
[HttpGet]
public async Task<ActionResult<IEnumerable<TodoItemResponse>>> GetAll(
    [FromQuery] bool? complete = null)  // ?complete=true or ?complete=false or omit for all
{
    var query = db.TodoItems.AsQueryable();

    if (complete is not null)
        query = query.Where(t => t.IsComplete == complete.Value);

    var todos = await query
        .OrderByDescending(t => t.CreatedAt)
        .Select(t => new TodoItemResponse(t.Id, t.Title, t.IsComplete, t.CreatedAt))
        .ToListAsync();

    return Ok(todos);
}
```

Test with: `GET /api/todos?complete=false` — returns only incomplete items.

### 10.2 Add a `POST /api/todos/{id}/complete` convenience endpoint

```csharp
[HttpPost("{id}/complete")]
public async Task<ActionResult<TodoItemResponse>> Complete(int id)
{
    var todo = await db.TodoItems.FindAsync(id);
    if (todo is null) return NotFound();

    todo.IsComplete = true;
    await db.SaveChangesAsync();

    return Ok(new TodoItemResponse(todo.Id, todo.Title, todo.IsComplete, todo.CreatedAt));
}
```

### 10.3 Add a `DueDate` to `TodoItem`

Add a nullable due date to the model:

```csharp
// In TodoItem.cs
public DateTime? DueDate { get; set; }
```

Update the DTOs to include it, then generate a new migration:

```bash
dotnet ef migrations add AddDueDate
dotnet ef database update
```

This is the full Alembic workflow in EF Core — model change → migration → apply.

### 10.4 Extract a service layer

Instead of calling `db` directly in the controller, add a `TodoService` that wraps EF Core queries. Register it with DI:

```csharp
// Program.cs
builder.Services.AddScoped<ITodoService, TodoService>();
```

Inject it into the controller instead of `AppDbContext`. This mirrors the Flutter repository pattern — the controller becomes thinner and the business logic is testable in isolation.

### 10.5 Write an integration test

Add a test project and use `WebApplicationFactory` to spin up the real API in memory:

```bash
cd ..
dotnet new xunit -n TodoApi.Tests
cd TodoApi.Tests
dotnet add reference ../TodoApi/TodoApi.csproj
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

```csharp
// TodoApiTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class TodosApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/todos");
        response.EnsureSuccessStatusCode();
    }
}
```

Run with `dotnet test`.

---

## Concepts Practised in This Phase

| ASP.NET / EF Core Concept | Where Used |
|---|---|
| `dotnet new webapi --use-controllers` | Scaffold step |
| `[ApiController]` attribute | Auto-validation, model binding |
| `[Route("api/[controller]")]` | Route prefix on the controller |
| `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` | HTTP method attributes |
| `ActionResult<T>` | Typed return that can also be 404, 204, etc. |
| `IActionResult` | Untyped return — used when no body (e.g. 204 NoContent) |
| `Ok(data)`, `NotFound()`, `CreatedAtAction(...)`, `NoContent()` | Helper methods on `ControllerBase` |
| `DbContext` + `DbSet<T>` | EF Core data access |
| `FindAsync(id)` | PK lookup — returns null if missing |
| `SaveChangesAsync()` | Flush pending changes to DB — like `db.session.commit()` |
| `[Required]`, `[MaxLength]` | Data Annotations for automatic validation |
| `record` types | Immutable DTOs |
| `builder.Services.AddDbContext<>()` | Register EF Core with the DI container |
| `builder.Configuration.GetConnectionString()` | Read from `appsettings.json` |
| `dotnet ef migrations add` / `dotnet ef database update` | Schema migrations — like Alembic |
| Swagger UI | Auto-generated interactive API docs |
| `[FromQuery]` | Bind query string params to method args |

---

## What's Next

With this API working, you're ready for **Phase 3 (Blazor Server)**. Here's how what you built maps forward:

| This project | Phase 3 (Blazor Server) |
|---|---|
| Swagger UI as the "frontend" | Replace with a Blazor `.razor` component |
| `TodosController` returns JSON | Blazor calls the API via `HttpClient` — or shares `AppDbContext` directly (Blazor Server can do both) |
| `AppDbContext` injected into controller | Same `AppDbContext` can be `@inject`ed into a Blazor component |
| `appsettings.json` connection string | Same config system in Blazor Server |

And in **Phase 4 (CQRS)** you'll replace the controller logic with MediatR `Command` and `Query` handlers — but the DB, the models, and the DTOs stay exactly the same.
