using sample.Application.Features.Todo.Dtos;
using sample.Domain.Entities;

namespace sample.Application.Features.Todo.Queries.GetAll;

public class GetallResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public List<TagDto> Tags { get; set; } = new List<TagDto>();
}
