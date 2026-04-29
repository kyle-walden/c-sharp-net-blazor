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