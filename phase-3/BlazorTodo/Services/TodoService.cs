// Services/TodoService.cs
using Microsoft.EntityFrameworkCore;
using BlazorTodo.Data;
using BlazorTodo.Models;

namespace BlazorTodo.Services;

public class TodoService(AppDbContext db) : ITodoService
{
    public async Task<List<TodoItem>> GetAllAsync() =>
        await db.TodoItems.OrderByDescending(t => t.CreatedAt).ToListAsync();

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
