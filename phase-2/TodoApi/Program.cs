// Flask analogy: app = Flask(__name__) + app.config + db.init_app(app)
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TodoApi.Data;
using Swashbuckle.AspNetCore.SwaggerUI;   // optional – only needed for the UI

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1",
        Description = "A simple Todo API built with .NET & Blazor",
    });

    // Optional: include XML documentation (<--> generate XML file)
    var xmlPath = System.IO.Path.Combine(
        AppContext.BaseDirectory,
        "TodoApi.xml");
    if (System.IO.File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // EnableSwagger UI at /swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();   // wire up all [Route] / [HttpGet] / [HttpPost] attributes

app.Run();