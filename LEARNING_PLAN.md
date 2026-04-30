# Crash Course: C# · ASP.NET Core Web API · Blazor Server · CQRS
> Tailored for developers who know Flutter, Firebase, Flask, and basic web fundamentals.
> Goal: Understand the stack end-to-end, build a simple app, and deploy it.

---

## Beginner Video Resources
- [C# Full Course for beginner](https://youtu.be/GhQdlIFylQ8?si=3cWHxg4JgoMItvO)
- [ASP .NET Core Web Api course](https://youtu.be/38GNKtclDdE?si=IgFCRKkJtUyp23k2)
- [Blazor Server side](https://youtu.be/8DNgdphLvag?si=NQ-rOVbyv65URVHR)
- [CQRS - Command Query Responsibility Segregation](https://youtu.be/yozD5Tnd8nw?si=3wWPlDBzQAPC-i3C)

## Index
- [Phase 1 — C# Language Fundamentals](#phase-1--c-language-fundamentals)
    - 1.1 Setup
    - 1.2 Core Language Concepts
    - 1.3 Key Differences from Dart/Python
    - 1.4 Practice Task (In-Memory Product Catalogue)
- [Phase 2 — ASP.NET Core Web API](#phase-2--aspnet-core-web-api)
    - 2.1 Create Your First API
    - 2.2 Project Structure
    - 2.3 Controller vs Flask Route
    - 2.4 Dependency Injection
    - 2.5 Entity Framework Core
    - 2.6 Minimal APIs
    - 2.7 Middleware Pipeline
    - 2.8 Configuration & Secrets
    - 2.9 Practice Task
- [Phase 3 — Blazor Server](#phase-3--blazor-server)
    - 3.1 Create a Blazor Server App
    - 3.2 Blazor Architecture vs Flutter
    - 3.3 Component Anatomy
    - 3.4 Component Parameters
    - 3.5 Forms & Validation
    - 3.6 Routing
    - 3.7 Layout
    - 3.8 Practice Task
- [Phase 4 — CQRS Pattern](#phase-4--cqrs-pattern)
    - 4.1 The Problem CQRS Solves
    - 4.2 CQRS with MediatR
    - 4.3 Query (Read)
    - 4.4 Command (Write)
    - 4.5 Dispatch from Controller or Blazor Component
    - 4.6 Adding Validation with FluentValidation
- [Phase 5 — Capstone App: "TaskBoard"](#phase-5--capstone-app-taskboard)
    - 5.1 Feature Scope
    - 5.2 Full Project Structure
    - 5.3 Solution Setup
    - 5.4 Domain Entity
    - 5.5 EF Core Setup
    - 5.6 CQRS Handlers
    - 5.7 API Controller
    - 5.8 Blazor Board Page
- [Phase 6 — Deployment](#phase-6--deployment)
    - Option A: Docker + Railway
    - Option B: Azure App Service
    - Option C: Self-hosted VPS
    - Production Checklist
- [Testing & QA](#testing--qa)

---

## Stack Analogy Map (Your Knowledge → This Stack)

| What you know | Equivalent here |
|---|---|
| Flutter widget tree | Blazor component tree (`.razor` files) |
| Flutter MVVM: View / ViewModel / Repository / Service | C# MVVM: `.razor` markup / `@code {}` block / Repository class / Service class |
| `ChangeNotifier` / Riverpod `Notifier` (ViewModel) | C# ViewModel class — or the `@code { }` block in a Blazor component |
| Dart class / `async/await` | C# class / `async/await` (near-identical syntax) |
| Flask route `@app.route(...)` | ASP.NET Controller `[HttpGet(...)]` or Minimal API `app.MapGet(...)` |
| Firebase Firestore | Entity Framework Core + SQL Server / SQLite |
| Provider / Riverpod state | Blazor cascading values, `StateHasChanged()`, or Fluxor |
| Firebase Auth | ASP.NET Identity / JWT Bearer auth |
| Flutter `pubspec.yaml` | .NET `*.csproj` + NuGet packages |
| `pip install` / `requirements.txt` | `dotnet add package` / `*.csproj` |

---

## Phase 1 — C# Language Fundamentals

> If you know Dart, C# will feel immediately familiar. Focus on the deltas.

### 1.1 Setup
- Install [.NET 8 SDK](https://dotnet.microsoft.com/download) — `dotnet --version` to verify
- Terminal install: 
```bash
# macOS (Terminal):
brew install --cask dotnet-sdk
```
- IDE: **Visual Studio 2022** (Windows) or **VS Code + C# Dev Kit extension** (cross-platform)
- Run your first program: `dotnet new console -n HelloCsharp && cd HelloCsharp && dotnet run`
- Permanently make sure the .NET SDK is installed and is on the path:
```bash
# macOS (Terminal):
echo 'export DOTNET_ROOT=$(brew --prefix dotnet)' >> ~/.zshrc
source ~/.zshrc
```

### 1.2 Core Language Concepts

#### Types & Variables
```csharp
// Like Dart, C# is strongly typed
string name = "Kyle";
int age = 30;
var inferred = 3.14;          // compiler infers double — same as Dart's var
const double Pi = 3.14159;    // compile-time constant (Dart: const)
```

#### Null Safety (same philosophy as Dart)
```csharp
string? maybeNull = null;     // ? means nullable — just like Dart
string definite = maybeNull ?? "fallback";  // ?? is identical to Dart
```

#### Classes & Constructors
```csharp
// Primary constructor (C# 12) — feels like Dart
public class Product(int id, string name, decimal price)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public decimal Price { get; set; } = price;
}
```

#### Records (immutable data — like Dart's copyWith pattern)
```csharp
public record ProductDto(int Id, string Name, decimal Price);
// Auto-generates equality, ToString, and a `with` expression:
var updated = product with { Price = 9.99m };
```

#### Async/Await (identical to Dart)
```csharp
// Flask: async def get_product(id): ...
// C#:
public async Task<Product> GetProductAsync(int id)
{
    var product = await _db.Products.FindAsync(id);
    return product ?? throw new KeyNotFoundException();
}
// Task<T>  ≈  Dart's Future<T>
// Task     ≈  Dart's Future<void>
```

#### LINQ (like Python list comprehensions / Flutter's `.where().map()`)
```csharp
// Python: [p for p in products if p.price > 10]
// use case: filtering, mapping, sorting collections
var expensive = products
    .Where(p => p.Price > 10)
    .OrderBy(p => p.Name)
    .Select(p => new { p.Id, p.Name })
    .ToList();
```

#### Interfaces & Dependency Injection (key pattern everywhere)
```csharp
// what this does: defines a contract (IProductRepository) that can have multiple implementations (e.g. InMemoryProductRepository, SqlProductRepository). The rest of the app depends on the interface, not the implementation — this allows for easy swapping of data sources and is essential for testing (mocking).
// Interface — defines the contract, no implementation
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
}
 // Implementation — can have multiple, e.g. InMemoryProductRepository, SqlProductRepository
public class ProductRepository : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync() { ... }
}
```

#### MVVM Architecture (same pattern as Flutter's recommended architecture)

You already use MVVM in Flutter. The same pattern applies in C# — it just looks slightly different depending on whether you're in a console app, ASP.NET, or Blazor.

| Layer | Responsibility | Flutter | C# (Console / Blazor) |
|---|---|---|---|
| **View** | Render UI, delegate events to ViewModel | Widget / `build()` | `Program.cs` output / `.razor` HTML markup |
| **ViewModel** | Hold UI state, talks to repositories. expose commands | `ChangeNotifier` / Riverpod `Notifier` | ViewModel class / Blazor `@code { }` block |
| **Model** | Domain entities | Dart class | C# class (e.g. `Product`) |
| **Repository** | Source of truth for app data | Repository class | Repository class (e.g. `IProductRepository`) |
| **Service** | Wraps external APIs / I/O | Service class | Service class / EF Core |

The key rules (same in Flutter and C#):
- **Views** know only about the ViewModel — never the Repository or Service directly. 
- **ViewModels** hold **state** (current list, error messages, loading flags) and expose **commands** (async methods the View calls on user actions). Processing logic, managing state, and calling services. Knows about Models and Repositories.
- **Models** defines data structures. Knows nothing about other layers.
- **Repositories** (Optional but recommended) Handles data fetching for this specific feature.
- **Services** wrap I/O (databases, HTTP APIs) and hold no state.

```csharp
// ViewModel — holds state + commands, no UI knowledge
public class ProductCatalogueViewModel(ProductService service)
{
    // State — like fields in a Flutter ChangeNotifier
    public IEnumerable<Product> Products { get; private set; } = [];
    public string ErrorMessage { get; private set; } = string.Empty;

    // Command — called by the View on user action
    public async Task LoadProductsAsync()
    {
        Products = await service.GetAllAsync();
        ErrorMessage = string.Empty;
    }

    public async Task AddProductAsync(string name, decimal price, string category)
    {
        try   { await service.CreateAsync(name, price, category); await LoadProductsAsync(); }
        catch (ArgumentException ex) { ErrorMessage = ex.Message; }
    }
}

// View (Program.cs / Blazor markup) — only talks to the ViewModel
var vm = new ProductCatalogueViewModel(service);
await vm.LoadProductsAsync();
foreach (var p in vm.Products) Console.WriteLine(p);  // render state
```

> In **Blazor Server** (Phase 3), the `@code { }` block *is* the ViewModel — it holds `_products`, `_newTitle`, and the `AddTodo()` command. 

### 1.3 Key Differences from Dart/Python to Note
- Semicolons are required
- `List<T>`, `Dictionary<K,V>`, `IEnumerable<T>` — generic collections
- No positional-only args; use object initializers or named params
- Access modifiers: `public`, `private`, `protected`, `internal`
- `using` = import (like `import` in Dart/Python)

### 1.4 Practice Task
Build a console app: a simple in-memory product catalogue (CRUD) using classes, LINQ, async methods, and the **MVVM pattern**. Structure your code across all four layers:
- **Model** — `Product` class
- **Repository** — `IProductRepository` interface + `InMemoryProductRepository` implementation
- **Service** — `ProductService` (business logic: validation, filtering, aggregation)
- **ViewModel** — `ProductCatalogueViewModel` (holds state + exposes commands)
- **View** — `Program.cs` (thin — only calls ViewModel commands and prints ViewModel state)

---

## Phase 2 — ASP.NET Core Web API

> Think: Flask but with a built-in DI container, middleware pipeline, and OpenAPI baked in.

### 2.1 Create Your First API
```bash
dotnet new webapi -n ProductApi --use-controllers
cd ProductApi
dotnet run
# Swagger UI auto-opens at https://localhost:{port}/swagger
```

### 2.2 Project Structure
```
ProductApi/
├── Controllers/        # Flask blueprints / route handlers
├── Models/             # Data models (like Pydantic models in Flask)
├── Services/           # Business logic layer
├── Data/               # EF Core DbContext (like SQLAlchemy)
├── Program.cs          # App bootstrap — Flask's app = Flask(__name__)
└── appsettings.json    # Config — like Flask's config.py / .env
```

### 2.3 Controller vs Flask Route
```python
# Flask
@app.route("/products/<int:id>", methods=["GET"])
def get_product(id):
    product = db.session.get(Product, id)
    return jsonify(product)
```
```csharp
// ASP.NET Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;  // DI

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _service.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }
}
```

### 2.4 Dependency Injection (DI)
DI is first-class in ASP.NET — no separate library needed.
```csharp
// Program.cs — register services (like Flask extensions)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// AddScoped  = new instance per HTTP request (most common)
// AddSingleton = one instance for app lifetime
// AddTransient = new instance every time it's requested
```

### 2.5 Entity Framework Core (EF Core — like SQLAlchemy)
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Product> Products => Set<Product>();
}
```
```bash
# Migrations (like Flask-Migrate / Alembic)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 2.6 Minimal APIs (simpler — closer to Flask style)
```csharp
// Program.cs
app.MapGet("/api/products", async (AppDbContext db) =>
    await db.Products.ToListAsync());

app.MapPost("/api/products", async (ProductDto dto, AppDbContext db) =>
{
    var product = new Product { Name = dto.Name, Price = dto.Price };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
});
```

### 2.7 Middleware Pipeline (like Flask's before_request / after_request)
```csharp
app.UseHttpsRedirection();   // redirect HTTP → HTTPS
app.UseAuthentication();     // who are you?
app.UseAuthorization();      // what can you do?
app.UseExceptionHandler();   // global error handling
```

### 2.8 Configuration & Secrets
```json
// appsettings.json (like .env / config.py)
{
  "ConnectionStrings": {
    "Default": "Data Source=app.db"
  }
}
```
```csharp
var connString = builder.Configuration.GetConnectionString("Default");
```

### 2.9 Practice Task
Build a REST API for a simple Todo app with EF Core + SQLite: GET/POST/PUT/DELETE `/api/todos`.

---

## Phase 3 — Blazor Server

> Blazor Server = Flutter widgets but rendered as HTML. The server holds state; SignalR (WebSocket) syncs UI changes to the browser in real time.

### 3.1 Create a Blazor Server App
```bash
dotnet new blazorserver -n BlazorTodo
cd BlazorTodo
dotnet run
```

### 3.2 Blazor Architecture vs Flutter

| Flutter                                   | Blazor Server                         | MVVM Layer |
|---                                        |---                                    |---|
| `StatefulWidget` + `State`                | Razor component (`.razor`)            | View + ViewModel combined |
| `build()` method                          | `.razor` HTML markup                  | **View** |
| `ChangeNotifier` / Riverpod `Notifier`    | `@code { }` block                     | **ViewModel** |
| `setState(() { })`                        | `StateHasChanged()`                   | ViewModel notifies View |
| Repository class                          | Repository class                      | **Repository** (data layer) |
| Service class                             | Service class / EF Core               | **Service** (data layer) |
| `pubspec.yaml`                            | `*.csproj`                            | — |
| Navigator / GoRouter                      | Blazor Router (`<Router>`, `@page`)   | — |
| `FutureBuilder`                           | `await` in `OnInitializedAsync()`     | — |
| Provider / InheritedWidget                | Cascading Values, `[Inject]` DI       | — |

> In Blazor Server, the `.razor` file bundles the View (HTML markup) and ViewModel (`@code { }`) in one file — just like a Flutter `StatefulWidget` bundles its widget tree and state class. The MVVM separation is still real: the markup only renders state; the `@code` block holds state and commands.

### 3.3 Component Anatomy
```razor
@* TodoList.razor — like a Flutter StatefulWidget *@
@page "/todos"
@inject ITodoService TodoService   @* like Flutter's Provider.of<T>(context) *@

<h1>Todos</h1>

@if (_todos is null)
{
    <p>Loading...</p>   @* like FutureBuilder's waiting state *@
}
else
{
    <ul>
        @foreach (var todo in _todos)
        {
            <li>@todo.Title</li>   @* @variable = interpolation *@
        }
    </ul>
}

<input @bind="_newTitle" placeholder="New todo..." />
<button @onclick="AddTodo">Add</button>

@code {
    private List<TodoItem>? _todos;
    private string _newTitle = string.Empty;

    // like Flutter's initState()
    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    private async Task AddTodo()
    {
        await TodoService.AddAsync(_newTitle);
        _newTitle = string.Empty;
        _todos = await TodoService.GetAllAsync();
        // StateHasChanged() is called automatically after event handlers
    }
}
```

### 3.4 Component Parameters (like Flutter widget params)
```razor
@* ChildComponent.razor *@
<p>@Message</p>

@code {
    [Parameter] public string Message { get; set; } = string.Empty;
    [Parameter] public EventCallback OnClicked { get; set; }  // like VoidCallback
}
```
```razor
@* Parent usage *@
<ChildComponent Message="Hello!" OnClicked="HandleClick" />
```

### 3.5 Forms & Validation
```razor
<EditForm Model="_form" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    <InputText @bind-Value="_form.Title" />
    <ValidationMessage For="@(() => _form.Title)" />
    <button type="submit">Save</button>
</EditForm>
```

### 3.6 Routing (like Flutter GoRouter)
```razor
@page "/todos"          @* maps to /todos *@
@page "/todos/{Id:int}" @* route param *@

@code {
    [Parameter] public int Id { get; set; }
}
```

### 3.7 Layout (like Flutter's Scaffold)
```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<NavMenu />             @* sidebar *@
<main>
    @Body               @* page content rendered here *@
</main>
```

### 3.8 Practice Task
Build a Blazor Server app: a browser-based Todo manager backed by EF Core + SQLite. Decompose the UI into reusable components and cover all the key Blazor patterns:
- **`Todos.razor`** — main list page: `@inject`, `OnInitializedAsync`, `@bind`, `@onclick`, `@foreach`
- **`TodoCard.razor`** — child component: `[Parameter]`, `EventCallback<T>` firing back to parent
- **`AddTodoForm.razor`** — form component: `<EditForm>`, `<DataAnnotationsValidator>`, `<InputText>`, `<ValidationMessage>`
- **`TodoEdit.razor`** — edit page: `@page "/todos/{Id:int}/edit"`, `[Parameter]` route param, `NavigationManager`
- **`ITodoService` / `TodoService`** — EF Core service registered with `AddScoped<>` in `Program.cs`

---

## Testing & QA

This section maps Flutter's testing types (unit, widget, integration) to equivalent testing strategies and tools for C#/.NET (console apps, ASP.NET Core Web API, and Blazor).

### Overview — test pyramid & mapping
- Flutter **unit tests** → .NET **unit tests** (xUnit/NUnit/MSTest) for models, services, and ViewModels.
- Flutter **widget tests** → **bUnit** component tests for Blazor (`.razor` components) or ViewModel tests for console/desktop UI.
- Flutter **integration tests** → **ASP.NET integration tests** using `WebApplicationFactory<T>` / `TestServer`, and **E2E** tests via Playwright/Selenium for browser flows.

Follow the test pyramid: many fast unit tests, fewer component/integration tests, and a small set of full E2E tests.

### Tools & packages
- Unit tests: `xUnit`, `NUnit`, or `MSTest` + `Moq` for mocking + `FluentAssertions` for readable assertions.
- Blazor component tests: `bUnit` (component rendering, parameters, events, markup assertions).
- Integration tests: `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) + EF Core in-memory or SQLite for test DBs.
- End-to-end: Playwright (Node or .NET), or Selenium / Puppeteer.

### Test project structure (recommended)
```
tests/
    ProductCatalogue.Tests.Unit          # xUnit unit tests (services, viewmodels)
    ProductCatalogue.Tests.Integration   # WebApplicationFactory integration tests
    ProductCatalogue.Tests.E2E           # Playwright or Selenium E2E tests
```

### Quick examples

Unit test (xUnit + Moq) — testing `ProductService` validation:
```csharp
// ProductServiceTests.cs
using Xunit;
using Moq;
using ProductCatalogue.Repositories;
using ProductCatalogue.Services;

public class ProductServiceTests
{
        [Fact]
        public async Task CreateAsync_InvalidName_ThrowsArgumentException()
        {
                var repo = new Mock<IProductRepository>();
                var service = new ProductService(repo.Object);

                await Assert.ThrowsAsync<ArgumentException>(() =>
                        service.CreateAsync("", 1m, "cat"));
        }
}
```

Component test (bUnit) — Blazor component rendering check:
```csharp
// ProductListTests.cs
using Bunit;
using Xunit;

public class ProductListTests : TestContext
{
        [Fact]
        public void RendersProducts()
        {
                var products = new[] { new Product { Id = 1, Name = "Key", Price = 1m } };
                var cut = RenderComponent<ProductList>(parameters =>
                        parameters.Add(p => p.Products, products));

                Assert.Contains("Key", cut.Markup);
        }
}
```

Integration test (WebApplicationFactory) — exercise the real HTTP pipeline:
```csharp
// ProductsApiTests.cs
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
        private readonly HttpClient _client;
        public ProductsApiTests(WebApplicationFactory<Program> factory)
        {
                _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ReturnsOk()
        {
                var res = await _client.GetAsync("/api/products");
                res.EnsureSuccessStatusCode();
        }
}
```

E2E with Playwright (quick notes)
- Use Playwright to automate browser flows (login, CRUD through UI).
- Two approaches: Playwright Node (recommended for full example suites) or Playwright for .NET.

Installation examples:
```bash
dotnet add package xunit
dotnet add package Moq
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package bunit
# For Playwright (Node):
npm init playwright@latest
npx playwright test
```

### Running tests
- Run all tests: `dotnet test`
- Run a specific test project: `dotnet test tests/ProductCatalogue.Tests.Unit`
- Use test categories or traits to filter slow/integration tests in CI.

### Strategies & best practices
- Keep unit tests fast and isolated (mock dependencies).
- Test behavior, not implementation details.
- Use in-memory or ephemeral SQLite for integration tests to simulate EF Core.
- Reserve E2E tests for critical user journeys only.
- Seed deterministic test data and reset state between tests (use fixtures or Respawn for DB resets).
- Add tests as you add features — consider TDD for critical logic.

### CI suggestion (GitHub Actions)
```yaml
name: CI
on: [push, pull_request]
jobs:
    build-and-test:
        runs-on: ubuntu-latest
        steps:
            - uses: actions/checkout@v4
            - name: Setup .NET
                uses: actions/setup-dotnet@v3
                with:
                    dotnet-version: '8.0.x'
            - name: Restore
                run: dotnet restore
            - name: Build
                run: dotnet build --no-restore --configuration Release
            - name: Run unit + integration tests
                run: dotnet test --no-build --verbosity normal
            # Add Playwright step if using Node Playwright (optional)
```

---


---

## Phase 4 — CQRS Pattern

Command Query Responsibility Segregation (CQRS) is an architectural pattern that separates read operations (queries) from write operations (commands). Instead of having a single service class that handles both getting and modifying data, you create distinct handler classes for each operation. This leads to clearer separation of concerns, easier scaling and optimization for reads vs writes, and better organization as the app grows in complexity.

> CQRS = separating reads (Queries) from writes (Commands). In Flask terms: instead of one service with get/create/update methods, you have separate handler classes per operation. Uses the **MediatR** library.
> Benefits: clear separation of concerns, easier to scale and optimize reads vs writes, and better organization as the app grows.
> Drawback: more boilerplate and complexity for simple apps — best for medium+ complexity.

### 4.1 The Problem CQRS Solves
```
Traditional service (Flask style):
ProductService.get_all()      ← read
ProductService.create(data)   ← write
ProductService.update(data)   ← write
ProductService.delete(id)     ← write

Problem: as the app grows, this one class balloons. Reads and writes
have different scaling, caching, and validation needs.
```

### 4.2 CQRS with MediatR
```bash
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
```
```csharp
// Program.cs
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

### 4.3 Query (Read)
```csharp
// 1. Define the query (the "request")
public record GetAllTodosQuery : IRequest<IEnumerable<TodoDto>>;

// 2. Define the handler
public class GetAllTodosHandler : IRequestHandler<GetAllTodosQuery, IEnumerable<TodoDto>>
{
    private readonly AppDbContext _db;
    public GetAllTodosHandler(AppDbContext db) => _db = db;

    public async Task<IEnumerable<TodoDto>> Handle(
        GetAllTodosQuery request, CancellationToken ct)
    {
        return await _db.Todos
            .Select(t => new TodoDto(t.Id, t.Title, t.IsComplete))
            .ToListAsync(ct);
    }
}
```

### 4.4 Command (Write)
```csharp
// 1. Define the command
public record CreateTodoCommand(string Title) : IRequest<TodoDto>;

// 2. Define the handler
public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, TodoDto>
{
    private readonly AppDbContext _db;
    public CreateTodoHandler(AppDbContext db) => _db = db;

    public async Task<TodoDto> Handle(CreateTodoCommand request, CancellationToken ct)
    {
        var todo = new Todo { Title = request.Title };
        _db.Todos.Add(todo);
        await _db.SaveChangesAsync(ct);
        return new TodoDto(todo.Id, todo.Title, todo.IsComplete);
    }
}
```

### 4.5 Dispatch from Controller or Blazor Component
```csharp
// Controller
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly IMediator _mediator;
    public TodosController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _mediator.Send(new GetAllTodosQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTodoCommand command)
        => Ok(await _mediator.Send(command));
}
```
```razor
@* Blazor component *@
@inject IMediator Mediator

@code {
    private IEnumerable<TodoDto>? _todos;

    protected override async Task OnInitializedAsync()
        => _todos = await Mediator.Send(new GetAllTodosQuery());
}
```

### 4.6 Adding Validation with FluentValidation
```bash
dotnet add package FluentValidation.DependencyInjectionExtensions
```
```csharp
public class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
```

---

## Phase 5 — Capstone App: "TaskBoard"

A minimal Kanban-style task board with:
- **Backend**: ASP.NET Core Web API + EF Core + SQLite
- **Frontend**: Blazor Server
- **Pattern**: CQRS via MediatR
- **Auth**: Simple JWT or ASP.NET Identity (cookie-based for Blazor)
- **Deploy**: Docker + Railway / Azure App Service

### 5.1 Feature Scope
- [ ] Create / view / complete / delete tasks
- [ ] Tasks belong to columns: To Do, In Progress, Done
- [ ] Simple user login (cookie auth)

### 5.2 Full Project Structure

Each project maps to a distinct MVVM layer:

```
TaskBoard/
├── TaskBoard.Api/              # VIEW layer — routes HTTP requests to Application layer
│   ├── Controllers/            #   Controllers dispatch Commands/Queries (thin ViewModels for REST)
│   ├── Program.cs
│   └── appsettings.json
├── TaskBoard.Application/      # VIEWMODEL layer — CQRS Commands, Queries, Handlers, Validators
│   ├── Tasks/
│   │   ├── Commands/
│   │   │   ├── CreateTaskCommand.cs
│   │   │   └── CreateTaskHandler.cs
│   │   └── Queries/
│   │       ├── GetTasksQuery.cs
│   │       └── GetTasksHandler.cs
│   └── Common/
├── TaskBoard.Domain/           # MODEL layer — plain C# domain entities
│   └── Entities/
│       └── TaskItem.cs
├── TaskBoard.Infrastructure/   # SERVICE/REPOSITORY layer — EF Core DbContext, data access
│   └── Data/
│       └── AppDbContext.cs
└── TaskBoard.Web/              # VIEW + VIEWMODEL layer — Blazor Server
    ├── Pages/                  #   .razor files: markup = View, @code block = ViewModel
    ├── Shared/
    └── Program.cs
```

> **MVVM ↔ CQRS**: CQRS (Phase 4) is a natural extension of MVVM. Each `Command` or `Query` is a formalised ViewModel command — instead of calling `viewModel.CreateTask(title)`, you dispatch `new CreateTaskCommand(title)` through MediatR. The Handler is the implementation. The layers stay the same; the wiring becomes more explicit and testable.

### 5.3 Solution Setup
```bash
dotnet new sln -n TaskBoard
dotnet new webapi -n TaskBoard.Api
dotnet new blazorserver -n TaskBoard.Web
dotnet new classlib -n TaskBoard.Application
dotnet new classlib -n TaskBoard.Domain
dotnet new classlib -n TaskBoard.Infrastructure

dotnet sln add **/*.csproj

# Add project references
cd TaskBoard.Api
dotnet add reference ../TaskBoard.Application ../TaskBoard.Infrastructure

cd ../TaskBoard.Web
dotnet add reference ../TaskBoard.Application ../TaskBoard.Infrastructure

cd ../TaskBoard.Infrastructure
dotnet add reference ../TaskBoard.Domain

cd ../TaskBoard.Application
dotnet add reference ../TaskBoard.Domain
```

### 5.4 Domain Entity
```csharp
// TaskBoard.Domain/Entities/TaskItem.cs
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Column { get; set; } = "Todo";  // Todo | InProgress | Done
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 5.5 EF Core Setup
```csharp
// TaskBoard.Infrastructure/Data/AppDbContext.cs
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}
```
```csharp
// Register in Api/Program.cs and Web/Program.cs
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=taskboard.db"));
```
```bash
cd TaskBoard.Infrastructure
dotnet ef --startup-project ../TaskBoard.Api migrations add Init
dotnet ef --startup-project ../TaskBoard.Api database update
```

### 5.6 CQRS Handlers (Application Layer)
```csharp
// Commands/CreateTask/CreateTaskCommand.cs
public record CreateTaskCommand(string Title) : IRequest<TaskItemDto>;

// Commands/CreateTask/CreateTaskHandler.cs
public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    private readonly AppDbContext _db;
    public CreateTaskHandler(AppDbContext db) => _db = db;
    public async Task<TaskItemDto> Handle(CreateTaskCommand req, CancellationToken ct)
    {
        var task = new TaskItem { Title = req.Title };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return new TaskItemDto(task.Id, task.Title, task.Column);
    }
}

// Queries/GetTasks/GetTasksQuery.cs
public record GetTasksQuery : IRequest<IEnumerable<TaskItemDto>>;

// Queries/GetTasks/GetTasksHandler.cs
public class GetTasksHandler : IRequestHandler<GetTasksQuery, IEnumerable<TaskItemDto>>
{
    private readonly AppDbContext _db;
    public GetTasksHandler(AppDbContext db) => _db = db;
    public async Task<IEnumerable<TaskItemDto>> Handle(GetTasksQuery req, CancellationToken ct)
        => await _db.Tasks.Select(t => new TaskItemDto(t.Id, t.Title, t.Column)).ToListAsync(ct);
}

// Shared DTO
public record TaskItemDto(int Id, string Title, string Column);
```

### 5.7 API Controller
```csharp
[ApiController, Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    public TasksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]   public async Task<IActionResult> Get()
        => Ok(await _mediator.Send(new GetTasksQuery()));

    [HttpPost]  public async Task<IActionResult> Create(CreateTaskCommand cmd)
        => Ok(await _mediator.Send(cmd));
}
```

### 5.8 Blazor Board Page
```razor
@page "/"
@inject IMediator Mediator

<h1>TaskBoard</h1>

<div style="display:flex; gap:1rem;">
    @foreach (var column in new[] { "Todo", "InProgress", "Done" })
    {
        <div style="flex:1; border:1px solid #ccc; padding:1rem;">
            <h3>@column</h3>
            @foreach (var task in _tasks?.Where(t => t.Column == column) ?? [])
            {
                <div style="padding:0.5rem; margin:0.25rem; background:#f5f5f5;">
                    @task.Title
                </div>
            }
        </div>
    }
</div>

<input @bind="_newTitle" placeholder="New task title..." />
<button @onclick="AddTask">Add to Todo</button>

@code {
    private IEnumerable<TaskItemDto>? _tasks;
    private string _newTitle = string.Empty;

    protected override async Task OnInitializedAsync()
        => _tasks = await Mediator.Send(new GetTasksQuery());

    private async Task AddTask()
    {
        if (string.IsNullOrWhiteSpace(_newTitle)) return;
        await Mediator.Send(new CreateTaskCommand(_newTitle));
        _newTitle = string.Empty;
        _tasks = await Mediator.Send(new GetTasksQuery());
    }
}
```

---

## Phase 6 — Deployment

### Option A: Docker + Railway (Easiest — like deploying a Flask container)

#### Dockerfile (Blazor Server / combined app)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish TaskBoard.Web/TaskBoard.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskBoard.Web.dll"]
```

```bash
docker build -t taskboard .
docker run -p 8080:8080 taskboard
```

**Deploy to Railway:**
1. Push to GitHub
2. New project → Deploy from GitHub repo → Railway auto-detects Dockerfile
3. Set `PORT` env var → done

### Option B: Azure App Service (if client uses Azure)
```bash
dotnet publish -c Release -o ./publish
az webapp up --name taskboard-app --resource-group myRG --runtime "DOTNET|8.0"
```

### Option C: Self-hosted Linux VPS
```bash
# On your server
sudo apt install dotnet-runtime-8.0
scp -r ./publish user@server:/var/www/taskboard
# Configure nginx as reverse proxy + systemd service
```

### Production Checklist
- [ ] Move SQLite → PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL` package)
- [ ] Store connection string in environment variables (never appsettings.json in prod)
- [ ] Enable HTTPS (handled by platform on Railway/Azure)
- [ ] Add `builder.Services.AddResponseCompression()` for Blazor Server performance
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`

---



## Key Resources

- [Microsoft Learn – C# Fundamentals](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core/)
- [Blazor Docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [Nick Chapsas (YouTube)](https://www.youtube.com/@nickchapsas) — best .NET crash course content
- [Tim Corey (YouTube)](https://www.youtube.com/@IAmTimCorey) — beginner-friendly C#
- [Blazor Train (YouTube)](https://www.blazortrain.com/)
