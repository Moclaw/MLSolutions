using sample.Application.Features.Todo.Dtos;
using sample.Domain.Entities;

namespace sample.Application.Features.Todo.Queries.GetById;

public class GetByIdResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public ICollection<TagDto> Tags { get; set; } = [];

    public static GetByIdResponse FromEntity(TodoItem entity) => new GetByIdResponse
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        IsCompleted = entity.IsCompleted,
        CreatedAt = entity.CreatedAt,
        CompletedAt = entity.CompletedAt,
        CategoryId = entity.CategoryId,
        CategoryName = entity.Category?.Name,
        Tags = [.. entity.Tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            })]
    };
}
