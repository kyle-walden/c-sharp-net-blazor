// Models/TodoItemDto.cs
// DTOs (Data Transfer Objects) define the API's public contract.
// They're separate from the DB entity so you can change DB schema without breaking clients.
// Flask analogy: Pydantic models / Marshmallow schemas
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

// Response DTO — what the API sends back (includes Id and CreatedAt)
public record TodoItemResponse(int Id, string Title, bool IsComplete, DateTime CreatedAt);

// Create DTO — what the client sends to POST (no Id — DB generates it)
public record CreateTodoRequest(
    [Required][MaxLength(200)] string Title
);

// Update DTO — what the client sends to PUT (can update title and/or completion)
public record UpdateTodoRequest(
    [MaxLength(200)] string? Title,    // null = don't change
    bool? IsComplete                   // null = don't change
);