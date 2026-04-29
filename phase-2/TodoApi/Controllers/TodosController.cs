// Controllers/TodosController.cs
// Flask analogy: a Blueprint with @app.route decorators
// Each method = one Flask route function
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]                 // enables automatic 400 validation responses, model binding
[Route("api/[controller]")]     // [controller] → "todos" (class name minus "Controller")
public class TodosController(AppDbContext db) : ControllerBase
{
    // ---------------------------------------------------------------
    // GET /api/todos — list all todos
    // Flask: @bp.route("/todos", methods=["GET"])
    // ---------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemResponse>>> GetAll()
    {
        var todos = await db.TodoItems
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoItemResponse(t.Id, t.Title, t.IsComplete, t.CreatedAt))
            .ToListAsync();

        return Ok(todos);
    }

    // ---------------------------------------------------------------
    // GET /api/todos/{id} — get a single todo
    // Flask: @bp.route("/todos/<int:id>", methods=["GET"])
    // ---------------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemResponse>> GetById(int id)
    {
        var todo = await db.TodoItems.FindAsync(id);

        if (todo is null)
            return NotFound();  // 404 — like Flask's abort(404)

        return Ok(new TodoItemResponse(todo.Id, todo.Title, todo.IsComplete, todo.CreatedAt));
    }

    // ---------------------------------------------------------------
    // POST /api/todos — create a new todo
    // Flask: @bp.route("/todos", methods=["POST"])
    // ---------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<TodoItemResponse>> Create(CreateTodoRequest request)
    {
        // [ApiController] already validated [Required] / [MaxLength] before we get here
        var todo = new TodoItem
        {
            Title = request.Title
            // IsComplete defaults to false, CreatedAt defaults to UtcNow
        };

        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();    // like db.session.commit() in Flask

        var response = new TodoItemResponse(todo.Id, todo.Title, todo.IsComplete, todo.CreatedAt);

        // 201 Created with a Location header pointing to GET /api/todos/{id}
        // Flask equivalent: return jsonify(response), 201, {"Location": f"/api/todos/{todo.id}"}
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, response);
    }

    // ---------------------------------------------------------------
    // PUT /api/todos/{id} — update title and/or completion status
    // Flask: @bp.route("/todos/<int:id>", methods=["PUT"])
    // ---------------------------------------------------------------
    [HttpPut("{id}")]
    public async Task<ActionResult<TodoItemResponse>> Update(int id, UpdateTodoRequest request)
    {
        var todo = await db.TodoItems.FindAsync(id);

        if (todo is null)
            return NotFound();

        // Only update fields that the client actually sent (non-null)
        if (request.Title is not null)
            todo.Title = request.Title;

        if (request.IsComplete is not null)
            todo.IsComplete = request.IsComplete.Value;

        await db.SaveChangesAsync();

        return Ok(new TodoItemResponse(todo.Id, todo.Title, todo.IsComplete, todo.CreatedAt));
    }

    // ---------------------------------------------------------------
    // DELETE /api/todos/{id} — delete a todo
    // Flask: @bp.route("/todos/<int:id>", methods=["DELETE"])
    // ---------------------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await db.TodoItems.FindAsync(id);

        if (todo is null)
            return NotFound();

        db.TodoItems.Remove(todo);
        await db.SaveChangesAsync();

        return NoContent();  // 204 — the standard success response for DELETE
    }
}