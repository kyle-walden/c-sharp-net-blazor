# Phase 3.8 — Blazor Server Todo App

> Goal: Build a Blazor Server app that manages Todos with a full browser UI using **components**, **component parameters**, **forms & validation**, **routing**, and **DI-injected services** backed by EF Core + SQLite.  
> Analogies from your Flutter/Flask background are included throughout.

---

## What You're Building

A browser-based Todo app running on Blazor Server — the state lives on the server, and UI changes are pushed to the browser over a SignalR WebSocket connection (no page reloads, no JSON round-trips from the browser).

| Blazor Concept | What you'll build | Flutter analogy |
|---|---|---|
| **Component** | `Todos.razor` page, `TodoCard.razor`, `AddTodoForm.razor` | `StatefulWidget` |
| **@inject** | Inject `ITodoService` into pages | `Provider.of<T>(context)` |
| **@code { }** | State + commands inside each component | `State<T>` class |
| **[Parameter]** | Pass data from parent to child component | Widget constructor params |
| **EventCallback** | Child notifies parent of events | `VoidCallback` / `Function()` |
| **EditForm** | Validated form for adding/editing todos | Flutter `Form` + `GlobalKey<FormState>` |
| **@page + route params** | `@page "/todos/{Id:int}/edit"` | GoRouter path params |
| **Layout** | `MainLayout.razor` wraps every page | Flutter `Scaffold` |
| **OnInitializedAsync** | Load data when component mounts | `initState()` + `FutureBuilder` |

> The `.razor` file bundles the View (HTML markup) and ViewModel (`@code { }`) in one file — just like a Flutter `StatefulWidget` bundles its widget tree and `State` class. You already know this pattern.

No separate API server. The Blazor Server app accesses EF Core directly — the server-side C# is always available in memory.

---

## [x] Step 1 — Scaffold the Project

```bash
cd phase-3
dotnet new blazor -n BlazorTodo # flutter: flutter create --template=app
cd BlazorTodo
```

Open in VS Code:
```bash
code .
```

The template generates quite a bit of boilerplate. Delete the demo files you won't use:
```bash
rm Pages/Counter.razor Pages/FetchData.razor
rm Data/WeatherForecast.cs Data/WeatherForecastService.cs
```

> **Why delete them?** The template ships with a weather forecast demo. Removing it keeps the compiler quiet and the project structure clean as you build.

By the end you'll have this structure:
```
BlazorTodo/
├── Components/
│   ├── AddTodoForm.razor       # reusable validated form component
│   └── TodoCard.razor          # individual todo card (child component)
├── Data/
│   └── AppDbContext.cs         # EF Core DbContext — like SQLAlchemy db
├── Models/
│   └── TodoItem.cs             # domain entity + EF Core table
├── Pages/
│   ├── _Host.cshtml            # (generated) HTML shell — do not touch
│   ├── Index.razor             # home — redirects to /todos
│   ├── Todos.razor             # main list page
│   └── TodoEdit.razor          # edit a single todo (route param demo)
├── Services/
│   ├── ITodoService.cs         # contract — like a Dart abstract class
│   └── TodoService.cs          # EF Core implementation
├── Shared/
│   ├── MainLayout.razor        # wraps every page — like Flutter Scaffold
│   └── NavMenu.razor           # sidebar nav
├── _Imports.razor              # global @using directives
├── App.razor                   # router root
├── Program.cs                  # app bootstrap + DI registration, Flutter equivalent of main()
└── BlazorTodo.csproj
```

---

## [x] Step 2 — Install EF Core and Define the `TodoItem` Model

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

Create `Models/TodoItem.cs`:
```bash
mkdir Models
```

```csharp
// Models/TodoItem.cs
// The domain entity — EF Core maps this class to a "TodoItems" table in SQLite.
// Flutter analogy: your model class (the M in MVVM).
// Data Annotations serve double duty: EF Core uses them for the DB schema (NOT NULL,
// VARCHAR(200)) AND Blazor's <EditForm> uses them for form validation automatically.
using System.ComponentModel.DataAnnotations;

namespace BlazorTodo.Models;

public class TodoItem
{
    public int Id { get; set; }                             // PK — EF Core convention

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;      // NOT NULL, VARCHAR(200)

    public bool IsComplete { get; set; }                    // bool → INTEGER (0/1) in SQLite

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## [x] Step 3 — Create `AppDbContext` and Run Migrations

Create `Data/AppDbContext.cs`:
```bash
mkdir Data
```

```csharp
// Data/AppDbContext.cs
// The EF Core database session.
// Flask/SQLAlchemy analogy: db = SQLAlchemy(app) — the object you call queries on.
using Microsoft.EntityFrameworkCore;
using BlazorTodo.Models;

namespace BlazorTodo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSet<T> = a queryable table
    // Flask analogy: TodoItem.query.all() → db.TodoItems.ToListAsync()
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}
```

Add the connection string to `appsettings.json`:
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

Install the EF Core CLI tools if you haven't already (only needed once globally):
```bash
dotnet tool install --global dotnet-ef
```

Generate the first migration and create the database file:
```bash
dotnet ef migrations add InitialCreate # generates a migration based on the current model
dotnet ef database update # applies the migration to the database (creates todos.db with the TodoItems table)
```
A migration is a C# class that describes the schema changes needed to sync the database with your model. The `update` command applies those changes to the database. After running these,

You'll see a `Migrations/` folder and a `todos.db` file appear in the project directory.

> **Flask-Migrate analogy:**
> ```
> flask db migrate -m "InitialCreate"  →  dotnet ef migrations add InitialCreate
> flask db upgrade                     →  dotnet ef database update
> ```

---

## [x] Step 4 — Create the Service Layer

The service wraps EF Core queries and holds business logic. Blazor components will `@inject` this — they never touch `AppDbContext` directly. This keeps the component (ViewModel) layer thin.

Create `Services/ITodoService.cs`:
```bash
mkdir Services
```

```csharp
// Services/ITodoService.cs
// The interface — defines the contract. Components depend on this, not the implementation.
// Dart analogy: abstract class ITodoService { ... }
using BlazorTodo.Models;

namespace BlazorTodo.Services;

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task<TodoItem> CreateAsync(string title);
    Task<bool> ToggleCompleteAsync(int id);
    Task<bool> UpdateTitleAsync(int id, string newTitle);
    Task<bool> DeleteAsync(int id);
}
```

Create `Services/TodoService.cs`:
```csharp
// Services/TodoService.cs
// EF Core implementation of the service contract.
// Primary constructor injects AppDbContext via DI — same pattern as Phase 2.
using Microsoft.EntityFrameworkCore;
using BlazorTodo.Data;
using BlazorTodo.Models;

namespace BlazorTodo.Services;

public class TodoService(AppDbContext db) : ITodoService
{
    public async Task<List<TodoItem>> GetAllAsync() =>
        await db.TodoItems
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await db.TodoItems.FindAsync(id);

    public async Task<TodoItem> CreateAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        var item = new TodoItem { Title = title.Trim() };
        db.TodoItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> ToggleCompleteAsync(int id)
    {
        var item = await db.TodoItems.FindAsync(id);
        if (item is null) return false;

        item.IsComplete = !item.IsComplete;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateTitleAsync(int id, string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle)) return false;

        var item = await db.TodoItems.FindAsync(id);
        if (item is null) return false;

        item.Title = newTitle.Trim();
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await db.TodoItems.FindAsync(id);
        if (item is null) return false;

        db.TodoItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }
}
```

---

## [x] Step 5 — Register Services in `Program.cs`

Replace the generated `Program.cs` with:

```csharp
// Program.cs — app bootstrap + DI registration
// Flask analogy: app = Flask(__name__) + db.init_app(app) + blueprint registration
using Microsoft.EntityFrameworkCore;
using BlazorTodo.Data;
using BlazorTodo.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Register services (DI container) ---

// EF Core + SQLite (same pattern as Phase 2)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Register our service — AddScoped = one instance per SignalR circuit (one per browser tab)
// Flutter analogy: registering a provider/repository in a MultiProvider
builder.Services.AddScoped<ITodoService, TodoService>();

// Blazor Server infrastructure
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();               // SignalR WebSocket hub — the live connection to the browser
app.MapFallbackToPage("/_Host"); // all requests → Blazor's HTML shell

app.Run();
```

> **`AddScoped` in Blazor Server** means one service instance per browser-tab connection (SignalR circuit). Unlike ASP.NET Web API where scoped = per HTTP request, here the circuit is long-lived — so a scoped service persists as long as the tab is open. This matters for EF Core: each tab gets its own `DbContext` instance, which is correct.

---

## [x] Step 6 — Build the `Todos` List Page

This is the main page. It demonstrates `@inject`, `OnInitializedAsync`, `@bind`, and event handlers.

Create `Pages/Todos.razor`:

```razor
@* Pages/Todos.razor *@
@* Flutter analogy: a StatefulWidget — markup is build(), @code is the State<T> class *@
@page "/todos"
@inject ITodoService TodoService   @* like Provider.of<ITodoService>(context) *@
@using BlazorTodo.Models
@using BlazorTodo.Services

<PageTitle>Todos</PageTitle>

<h1>My Todos</h1>

@* Loading state — like FutureBuilder's ConnectionState.waiting *@
@if (_todos is null)
{
    <p><em>Loading...</em></p>
}
else
{
    @* Computed property read from state — no method call needed *@
    <p>@_todos.Count(t => !t.IsComplete) remaining</p>

    <ul class="list-unstyled">
        @foreach (var todo in _todos)
        {
            <li class="d-flex align-items-center gap-2 mb-2">
                <input type="checkbox"
                       checked="@todo.IsComplete"
                       @onchange="() => Toggle(todo.Id)" />
                <span style="@(todo.IsComplete ? "text-decoration:line-through;color:gray;" : "")">
                    @todo.Title
                </span>
                <a href="/todos/@todo.Id/edit" class="btn btn-sm btn-outline-secondary">Edit</a>
                <button class="btn btn-sm btn-outline-danger"
                        @onclick="() => Delete(todo.Id)">
                    Delete
                </button>
            </li>
        }
    </ul>

    @if (_todos.Count == 0)
    {
        <p>No todos yet — add one below.</p>
    }
}

@* Inline add form — we'll extract this to a component in Step 8 *@
<hr />
<div class="d-flex gap-2">
    <input class="form-control" style="max-width:300px;"
           @bind="_newTitle"
           @bind:event="oninput"
           placeholder="New todo title..." />
    <button class="btn btn-primary"
            @onclick="AddTodo"
            disabled="@string.IsNullOrWhiteSpace(_newTitle)">
        Add
    </button>
</div>

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <p class="text-danger mt-2">@_errorMessage</p>
}

@code {
    // STATE — like fields in a Flutter State<T> class
    // null = not yet loaded (shows "Loading..."), empty list = loaded but empty
    private List<TodoItem>? _todos;
    private string _newTitle = string.Empty;
    private string _errorMessage = string.Empty;

    // LIFECYCLE — like Flutter's initState()
    // Called once when the component first renders; awaits async data before rendering
    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    // COMMANDS — called by UI events above
    // StateHasChanged() is called automatically after Blazor event handlers,
    // so the UI re-renders as soon as these methods return.

    private async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(_newTitle)) return;

        try
        {
            await TodoService.CreateAsync(_newTitle);
            _newTitle = string.Empty;
            _errorMessage = string.Empty;
            _todos = await TodoService.GetAllAsync();   // refresh state — like notifyListeners()
        }
        catch (ArgumentException ex)
        {
            _errorMessage = ex.Message;
        }
    }

    private async Task Toggle(int id)
    {
        await TodoService.ToggleCompleteAsync(id);
        _todos = await TodoService.GetAllAsync();
    }

    private async Task Delete(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos = await TodoService.GetAllAsync();
    }
}
```

**Key Blazor syntax introduced here:**

| Syntax | What it does | Flutter equivalent |
|---|---|---|
| `@inject ITodoService TodoService` | DI — inject the service | `context.read<ITodoService>()` |
| `@if (_todos is null)` | Conditional rendering | `snapshot.connectionState == waiting` |
| `@foreach (var todo in _todos)` | List rendering | `ListView.builder` |
| `@bind="_newTitle"` | Two-way binding to a field | `TextEditingController` |
| `@bind:event="oninput"` | Update on every keystroke, not just on blur | `onChanged` on `TextField` |
| `@onclick="AddTodo"` | Event handler | `onPressed` |
| `() => Toggle(todo.Id)` | Lambda captures loop variable | `() => onToggle(todo.id)` |
| `OnInitializedAsync()` | Runs once on mount, async | `initState()` + `FutureBuilder` |

---

## [!] Step 7 — Extract a `TodoCard` Child Component

This demonstrates **[Parameter]** and **EventCallback** — the Blazor equivalents of Flutter widget constructor params and `VoidCallback`.

Create `Components/TodoCard.razor`:
```bash
mkdir Components
```

```razor
@* Components/TodoCard.razor *@
@* Flutter analogy: a reusable widget that takes parameters and fires callbacks to its parent *@
@using BlazorTodo.Models

<li class="d-flex align-items-center gap-2 mb-2">
    <input type="checkbox"
           checked="@Todo.IsComplete"
           @onchange="HandleToggle" />
    <span style="@(Todo.IsComplete ? "text-decoration:line-through;color:gray;" : "")">
        @Todo.Title
    </span>
    <a href="/todos/@Todo.Id/edit" class="btn btn-sm btn-outline-secondary">Edit</a>
    <button class="btn btn-sm btn-outline-danger"
            @onclick="HandleDelete">
        Delete
    </button>
</li>

@code {
    // [Parameter] = widget constructor parameter in Flutter
    // The parent sets this when it writes <TodoCard Todo="item" />
    [Parameter] public TodoItem Todo { get; set; } = default!;

    // EventCallback<T> = VoidCallback / Function(T) in Flutter
    // The parent passes a handler; this component invokes it on user action.
    // EventCallback automatically triggers StateHasChanged() on the parent — you don't need to.
    [Parameter] public EventCallback<int> OnToggle { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }

    private async Task HandleToggle() => await OnToggle.InvokeAsync(Todo.Id);
    private async Task HandleDelete() => await OnDelete.InvokeAsync(Todo.Id);
}
```

Now update `Pages/Todos.razor` to use the new component. Replace the `<ul>...</ul>` block:

```razor
@* Replace the <ul>...</ul> block in Todos.razor with: *@
@using BlazorTodo.Components

<ul class="list-unstyled">
    @foreach (var todo in _todos)
    {
        <TodoCard Todo="todo"
                  OnToggle="Toggle"
                  OnDelete="Delete" />
    }
</ul>
```

> **Why `EventCallback<T>` instead of `Action<T>`?**  
> `EventCallback<T>` is Blazor-aware. After the parent's handler runs, it automatically calls `StateHasChanged()` on the parent so the UI re-renders. With plain `Action<T>` you'd have to call `StateHasChanged()` yourself. Always use `EventCallback` for component events.

---

## [x] Step 8 — Add an `AddTodoForm` Component with Validation

This demonstrates `<EditForm>`, `<DataAnnotationsValidator>`, and `<ValidationMessage>` — the Blazor equivalent of Flutter's `Form` + validators.

Create `Components/AddTodoForm.razor`:

```razor
@* Components/AddTodoForm.razor *@
@* Flutter analogy: a Form widget with validators and a submit callback *@
@using System.ComponentModel.DataAnnotations

@* EditForm binds to a model object (not a plain string).
   OnValidSubmit only fires when DataAnnotationsValidator passes all rules. *@
<EditForm Model="_model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />   @* connects [Required]/[MaxLength] on _model to the form *@

    <div class="d-flex gap-2 align-items-start">
        <div>
            @* InputText = TextFormField in Flutter; @bind-Value is two-way *@
            <InputText class="form-control"
                       @bind-Value="_model.Title"
                       placeholder="New todo title..." />
            @* ValidationMessage renders the error text when [Required]/[MaxLength] fails *@
            <ValidationMessage For="@(() => _model.Title)" />
        </div>
        <button type="submit" class="btn btn-primary">Add</button>
    </div>
</EditForm>

@code {
    // The form model — EditForm tracks this object's state, not individual fields
    private FormModel _model = new();

    // EventCallback fires when the form is submitted with valid data
    [Parameter] public EventCallback<string> OnAdd { get; set; }

    private async Task HandleValidSubmit()
    {
        // Only reached when [Required] and [MaxLength] pass
        await OnAdd.InvokeAsync(_model.Title);
        _model = new FormModel();   // reset the form fields
    }

    // Private nested class — keeps the form model close to the component that owns it
    private class FormModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
        public string Title { get; set; } = string.Empty;
    }
}
```

Now update `Pages/Todos.razor` to use the new form. Replace the inline `<hr /><div>` add form section:

```razor
@* Replace the <hr /><div>...</div> inline add form section in Todos.razor with: *@
<hr />
<AddTodoForm OnAdd="HandleAdd" />

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <p class="text-danger mt-2">@_errorMessage</p>
}
```

Add the `HandleAdd` method to `@code { }` in `Todos.razor`, and remove the old `AddTodo` method and `_newTitle` field:

```csharp
// Replace AddTodo() with HandleAdd() in @code { }
// Also remove: private string _newTitle = string.Empty;
private async Task HandleAdd(string title)
{
    try
    {
        await TodoService.CreateAsync(title);
        _errorMessage = string.Empty;
        _todos = await TodoService.GetAllAsync();
    }
    catch (ArgumentException ex)
    {
        _errorMessage = ex.Message;
    }
}
```

> **`EditForm` validation flow:**  
> 1. User types in `<InputText>` — bound to `_model.Title`.  
> 2. User clicks Submit.  
> 3. `<DataAnnotationsValidator>` checks `[Required]` and `[MaxLength]` on `_model`.  
> 4. If invalid: `<ValidationMessage>` renders the error inline — `OnValidSubmit` does NOT fire.  
> 5. If valid: `HandleValidSubmit` runs — only ever called with good data.  
> This is the Blazor equivalent of Flutter's `formKey.currentState!.validate()`.

---

## [x] Step 9 — Add a `TodoEdit` Page with a Route Parameter

This demonstrates `@page` with a typed route parameter, `NavigationManager`, and `EditForm` for editing.

Create `Pages/TodoEdit.razor`:

```razor
@* Pages/TodoEdit.razor *@
@* Demonstrates: typed route param, NavigationManager, full EditForm *@
@page "/todos/{Id:int}/edit"
@inject ITodoService TodoService
@inject NavigationManager Nav    @* programmatic navigation — like GoRouter.of(context).go() *@
@using BlazorTodo.Models
@using BlazorTodo.Services
@using System.ComponentModel.DataAnnotations

<PageTitle>Edit Todo</PageTitle>

@if (_todo is null && !_notFound)
{
    <p><em>Loading...</em></p>
}
else if (_notFound)
{
    <p class="text-danger">Todo not found.</p>
    <a href="/todos">Back to list</a>
}
else
{
    <h1>Edit Todo</h1>

    <EditForm Model="_form" OnValidSubmit="HandleSave">
        <DataAnnotationsValidator />

        <div class="mb-3">
            <label class="form-label">Title</label>
            <InputText class="form-control" @bind-Value="_form.Title" />
            <ValidationMessage For="@(() => _form.Title)" />
        </div>

        <div class="mb-3 form-check">
            <InputCheckbox class="form-check-input" @bind-Value="_form.IsComplete" id="isComplete" />
            <label class="form-check-label" for="isComplete">Complete</label>
        </div>

        <button type="submit" class="btn btn-primary">Save</button>
        <a href="/todos" class="btn btn-secondary ms-2">Cancel</a>
    </EditForm>

    @if (!string.IsNullOrEmpty(_errorMessage))
    {
        <p class="text-danger mt-2">@_errorMessage</p>
    }
}

@code {
    // Route parameter — Blazor extracts {Id:int} from the URL and sets this property.
    // Flutter/GoRouter analogy: GoRouterState.of(context).pathParameters['id']
    [Parameter] public int Id { get; set; }

    private TodoItem? _todo;
    private bool _notFound;
    private string _errorMessage = string.Empty;
    private FormModel _form = new();

    protected override async Task OnInitializedAsync()
    {
        _todo = await TodoService.GetByIdAsync(Id);
        if (_todo is null)
        {
            _notFound = true;
            return;
        }
        // Pre-populate the form with the current values
        _form = new FormModel { Title = _todo.Title, IsComplete = _todo.IsComplete };
    }

    private async Task HandleSave()
    {
        if (_todo is null) return;

        await TodoService.UpdateTitleAsync(_todo.Id, _form.Title);

        // Only toggle if the value changed
        if (_form.IsComplete != _todo.IsComplete)
            await TodoService.ToggleCompleteAsync(_todo.Id);

        // Navigate back to the list — like Flutter's context.go('/todos')
        Nav.NavigateTo("/todos");
    }

    private class FormModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
        public string Title { get; set; } = string.Empty;

        public bool IsComplete { get; set; }
    }
}
```

**New concepts introduced here:**

| Feature | What it does | Flutter analogy |
|---|---|---|
| `@page "/todos/{Id:int}/edit"` | Route with a typed int parameter | `GoRoute(path: '/todos/:id/edit')` |
| `[Parameter] public int Id` | Receives the extracted route param | `GoRouterState.pathParameters['id']` |
| `@inject NavigationManager Nav` | Programmatic navigation service | `GoRouter.of(context)` |
| `Nav.NavigateTo("/todos")` | Navigate programmatically after save | `context.go('/todos')` |
| `InputCheckbox` | Bound checkbox input | `Checkbox` widget |

---

## [x] Step 10 — Update NavMenu, Home Page, and `_Imports.razor`

**Update `Shared/NavMenu.razor`** — add a Todos link. Find the `<ul class="nav flex-column">` block and add a new `<li>` alongside the Home link:

```razor
@* Add this <li> inside the <ul class="nav flex-column"> in NavMenu.razor *@
<li class="nav-item px-3">
    <NavLink class="nav-link" href="todos">
        <span class="oi oi-task" aria-hidden="true"></span> Todos
    </NavLink>
</li>
```

**Update `Pages/Index.razor`** — redirect the home page to `/todos`:

```razor
@page "/"
@inject NavigationManager Nav

@code {
    protected override void OnInitialized()
    {
        Nav.NavigateTo("/todos", replace: true);
    }
}
```

**Update `_Imports.razor`** — add global `@using` directives so you don't repeat them in every file. Append to the existing entries:

```razor
@* Add to the bottom of _Imports.razor *@
@using BlazorTodo.Models
@using BlazorTodo.Services
@using BlazorTodo.Components
```

Once `_Imports.razor` has these, you can remove the individual `@using` lines from `Todos.razor`, `TodoEdit.razor`, and the `Components/` files.

---

## [x] Step 11 — Run It

```bash
dotnet run
```

The terminal will show:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

Open `https://localhost:7001` in your browser. You'll be redirected to `/todos`.

Test the full flow:

1. **Add a todo** using the form. Try submitting with an empty title — the `[Required]` validation should block it and show an error message inline (without a page reload).
2. **Check the checkbox** to mark it complete — it should get a strikethrough immediately.
3. **Click Edit** — you should navigate to `/todos/{id}/edit`.
4. **Change the title** and click Save — you should be returned to the list with the update visible.
5. **Delete a todo** — the list refreshes immediately.
6. **Open browser DevTools → Network tab** and look for a WebSocket connection to `/_blazor`. This is the SignalR circuit. Every click, every `@bind` change, every re-render happens over this one persistent connection — no individual HTTP requests, no page reloads.

--- Done! You have a fully functional Blazor Server app with EF Core, complete with a clean architecture and best practices.

## Step 12 — Stretch Goals (optional)

### 12.1 Add filter tabs (All / Active / Completed)

Add to `Todos.razor` above the `<ul>` block:
```razor
<div class="btn-group mb-3">
    <button class="btn @(_filter == "all" ? "btn-primary" : "btn-outline-primary")"
            @onclick="@(() => _filter = "all")">All</button>
    <button class="btn @(_filter == "active" ? "btn-primary" : "btn-outline-primary")"
            @onclick="@(() => _filter = "active")">Active</button>
    <button class="btn @(_filter == "done" ? "btn-primary" : "btn-outline-primary")"
            @onclick="@(() => _filter = "done")">Done</button>
</div>
```

Add to `@code { }`:
```csharp
private string _filter = "all";

private IEnumerable<TodoItem> FilteredTodos => _filter switch
{
    "active" => _todos?.Where(t => !t.IsComplete) ?? [],
    "done"   => _todos?.Where(t =>  t.IsComplete) ?? [],
    _        => _todos ?? []
};
```

Change `@foreach (var todo in _todos)` to `@foreach (var todo in FilteredTodos)`.

### 12.2 Add a "Clear completed" button

```razor
@if (_todos?.Any(t => t.IsComplete) == true)
{
    <button class="btn btn-outline-secondary btn-sm mt-2"
            @onclick="ClearCompleted">
        Clear completed
    </button>
}
```

```csharp
private async Task ClearCompleted()
{
    var completedIds = _todos?.Where(t => t.IsComplete).Select(t => t.Id).ToList() ?? [];
    foreach (var id in completedIds)
        await TodoService.DeleteAsync(id);
    _todos = await TodoService.GetAllAsync();
}
```

### 12.3 Add a `DueDate` field

Add `public DateTime? DueDate { get; set; }` to `TodoItem.cs`, add an `<InputDate>` to `AddTodoForm.razor` and `TodoEdit.razor`, then run a new migration:

```bash
dotnet ef migrations add AddDueDate
dotnet ef database update
```

This is the full EF Core migration workflow for a schema change — model change → migration → apply.

### 12.4 Connect to the Phase 2 API instead of EF Core directly

Instead of injecting an EF Core-backed `ITodoService`, create an `HttpTodoService` that calls the Phase 2 `TodoApi`:

```csharp
// Services/HttpTodoService.cs
public class HttpTodoService(HttpClient http) : ITodoService
{
    public async Task<List<TodoItem>> GetAllAsync() =>
        await http.GetFromJsonAsync<List<TodoItem>>("api/todos") ?? [];

    // ... implement other methods using http.PostAsJsonAsync, http.PutAsJsonAsync, etc.
}
```

Register it in `Program.cs` (replacing the existing `AddScoped<ITodoService, TodoService>()`):
```csharp
builder.Services.AddHttpClient<ITodoService, HttpTodoService>(client =>
    client.BaseAddress = new Uri("https://localhost:7234"));  // Phase 2 API port
```

Run the Phase 2 API in a separate terminal, then run this Blazor app. The Blazor component code stays identical — only the DI registration changes. This is the interface pattern paying off.

### 12.5 Write a bUnit component test

```bash
cd ..
dotnet new xunit -n BlazorTodo.Tests
cd BlazorTodo.Tests
dotnet add reference ../BlazorTodo/BlazorTodo.csproj
dotnet add package bunit
dotnet add package Moq
```

```csharp
// TodoCardTests.cs
using Bunit;
using Xunit;
using BlazorTodo.Components;
using BlazorTodo.Models;

public class TodoCardTests : TestContext
{
    [Fact]
    public void TodoCard_RendersTitle()
    {
        var todo = new TodoItem { Id = 1, Title = "Buy milk", IsComplete = false };
        var cut = RenderComponent<TodoCard>(p => p
            .Add(c => c.Todo, todo)
            .Add(c => c.OnToggle, EventCallback.Empty)
            .Add(c => c.OnDelete, EventCallback.Empty));

        Assert.Contains("Buy milk", cut.Markup);
    }

    [Fact]
    public void CompletedTodo_HasStrikethrough()
    {
        var todo = new TodoItem { Id = 1, Title = "Done task", IsComplete = true };
        var cut = RenderComponent<TodoCard>(p => p
            .Add(c => c.Todo, todo)
            .Add(c => c.OnToggle, EventCallback.Empty)
            .Add(c => c.OnDelete, EventCallback.Empty));

        Assert.Contains("line-through", cut.Markup);
    }
}
```

Run with `dotnet test`.

---

## Concepts Practised in This Phase

| Blazor Concept | Where Used |
|---|---|
| `dotnet new blazorserver` | Scaffold step |
| `@page "/todos"` | Route declaration on `Todos.razor` |
| `@page "/todos/{Id:int}/edit"` | Route with typed int parameter on `TodoEdit.razor` |
| `[Parameter] public int Id` | Page component receives the route param value |
| `@inject ITodoService TodoService` | DI — inject a service into a component |
| `@inject NavigationManager Nav` | Programmatic navigation service |
| `OnInitializedAsync()` | Async lifecycle hook — loads data on first render |
| `@if` / `@foreach` | Conditional and list rendering |
| `@bind` / `@bind:event="oninput"` | Two-way data binding to a field |
| `@onclick="Handler"` / `() => Method(id)` | Event binding and lambda capturing |
| `[Parameter]` | Component input property set by the parent |
| `EventCallback<T>` | Child-to-parent event — auto triggers `StateHasChanged()` on parent |
| `<EditForm Model="_model" OnValidSubmit="...">` | Validated form |
| `<DataAnnotationsValidator />` | Wires Data Annotations to the form |
| `<InputText>`, `<InputCheckbox>` | Bound form input components |
| `<ValidationMessage For="...">` | Inline validation error display |
| `AddScoped<ITodoService, TodoService>()` | Register service with DI (scoped to SignalR circuit) |
| `AddDbContext<AppDbContext>()` | Register EF Core with DI |
| `dotnet ef migrations add` / `dotnet ef database update` | Schema migrations |
| `StateHasChanged()` | Trigger re-render (called automatically after event handlers) |
| `NavLink` | Active-aware navigation link in the sidebar |

---

## What's Next

With this Blazor app working, you're ready for **Phase 4 (CQRS with MediatR)**. Here's how what you built maps forward:

| This project | Phase 4 (CQRS) |
|---|---|
| `TodoService.GetAllAsync()` called directly | Replace with `await Mediator.Send(new GetAllTodosQuery())` |
| `TodoService.CreateAsync(title)` called directly | Replace with `await Mediator.Send(new CreateTodoCommand(title))` |
| Service class holds all logic in one place | Logic splits into focused `QueryHandler` / `CommandHandler` classes |
| `ITodoService` interface | Replaced by MediatR's `IRequest<T>` contracts |
| DI-registered service | MediatR handlers are registered automatically via `RegisterServicesFromAssembly` |

The Blazor component structure, routing, forms, and EF Core setup stay **exactly the same** — CQRS only changes how data flows behind the `@inject` call.
