using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Queries.GetById;
using Response = sample.Application.Features.Todo.Queries.GetById.Response;

namespace sample.API.Endpoints.Todo.Queries;

public class GetTodoByIdEndpoint(IMediator mediator)
    : SingleEndpointBase<Request, Response>(mediator)
{
    [HttpGet("api/todos/{id}")]
    public override async Task<Shared.Responses.Response<Response>> HandleAsync(
        Request req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
