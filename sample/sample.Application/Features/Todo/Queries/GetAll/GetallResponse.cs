using sample.Application.Features.Todo.Dtos;

namespace sample.Application.Features.Todo.Queries.GetAll;

public class GetallResponse
{
    public List<TodoItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
