using MinimalAPI.Attributes;
using MinimalAPI.Handlers;

namespace sample.Application.Features.Todo.Queries.GetById;

public class Request : IQueryRequest<Response>
{
    [FromRoute]
    public int Id { get; set; }
}
