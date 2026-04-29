// The EF Core database session.
// Flask/SQLAlchemy analogy: db = SQLAlchemy(app) — the object you call queries on.
using Microsoft.EntityFrameworkCore;
using BlazorTodo.Models;

namespace BlazorTodo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}