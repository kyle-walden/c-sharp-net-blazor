namespace TodoApi.Models;

public class TodoItem
{
    public int Id { get; set; }                            // Primary key — EF Core convention: property named "Id" → PK
    public string Title { get; set; } = string.Empty;     // NOT NULL column
    public bool IsComplete { get; set; }                   // bool → INTEGER (0/1) in SQLite
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}