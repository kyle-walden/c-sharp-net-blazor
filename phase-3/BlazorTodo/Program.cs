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