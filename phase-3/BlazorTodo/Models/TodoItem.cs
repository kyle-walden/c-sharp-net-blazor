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