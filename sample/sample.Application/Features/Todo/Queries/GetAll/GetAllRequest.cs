using MinimalAPI.Handlers;

namespace sample.Application.Features.Todo.Queries.GetAll;

public class GetAllRequest : IQueryCollectionRequest<GetallResponse>
{
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string OrderBy { get; set; } = "Id";
    public bool IsAscending { get; set; } = true;
}