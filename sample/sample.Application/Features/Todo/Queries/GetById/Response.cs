using sample.Domain.Entities;

namespace sample.Application.Features.Todo.Queries.GetById;

public class Response
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public static Response FromEntity(TodoItem entity)
    {
        return new Response
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            IsCompleted = entity.IsCompleted,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt
        };
    }
}
