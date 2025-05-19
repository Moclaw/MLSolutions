using MinimalAPI.Handlers;

namespace sample.Application.Features.Todo.Queries.GetById;

public class Request : IQueryRequest<Response>
{
    public int Id { get; set; }
}
