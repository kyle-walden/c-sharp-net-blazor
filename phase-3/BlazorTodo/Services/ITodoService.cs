// Services/ITodoService.cs
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
