using MinimalAPI.Handlers;

namespace sample.Application.Features.Todo.Queries.GetAllTags;
public class Request : IQueryCollectionRequest<Response>
{
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string OrderBy { get; set; } = "Name";
    public bool IsAscending { get; set; } = true;
}