# BlazorTodo (Phase 3)

Developer summary

Overview
- Server-side Blazor application that provides a UI for creating and managing Todo items.
- Persists data using EF Core + SQLite and exposes a responsive UI via SignalR (Blazor Server model).

What it does
- Renders a web UI for listing, creating, toggling and deleting todos.
- Uses a scoped `TodoService` that encapsulates EF Core operations; migrations are included to create the schema.

Architecture (MVVM mapping)
- View: Razor components under `Pages/` and `Components/` — these are the Views that render UI and bind to state.
- ViewModel: in Blazor the component class (the `@code` block or a code-behind) typically behaves as the ViewModel: it holds UI state and calls services. Components here act as the View + ViewModel unless you explicitly extract separate ViewModel classes.
- Model: `Models/TodoItem.cs` + `Data/AppDbContext.cs` — domain entities and persistence layer (EF Core).
- Service: `Services/TodoService.cs` / `ITodoService` — encapsulates business logic and database operations; components call the service to fetch/mutate Model data.
- Composition: `Program.cs` registers DI (DbContext, ITodoService) and maps Blazor endpoints (`MapBlazorHub`, `_Host`) — it's the app composition root.

Key files
- `Program.cs` — setup and routing (`MapBlazorHub`, `MapFallbackToPage("/_Host")`).
- `Data/AppDbContext.cs` — DbContext.
- `Services/TodoService.cs` — CRUD operations used by components.
- `Migrations/` — contains `InitialCreate` migration for `TodoItems` table.

How to run (developer)
Make sure you have the .NET SDK that matches the project target (`TargetFramework` = `net10.0`). From this folder:

```bash
cd phase-3/BlazorTodo
dotnet restore
dotnet build

# (optional) install EF CLI if you plan to inspect/apply migrations locally
dotnet tool install --global dotnet-ef
dotnet ef database update

dotnet run
```

Then open the URL the app prints (commonly `https://localhost:7xxx` or `http://localhost:5xxx`).

Dev notes
- The app stores data in `todos.db` by default (connection string in `appsettings.json`).
- A migration (`InitialCreate`) is included — run `dotnet ef database update` to create the schema before first run.
- Blazor Server requires the server process to run while you interact with the UI in your browser.

