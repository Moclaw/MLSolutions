using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Queries.GetById;
using Response = sample.Application.Features.Todo.Queries.GetById.Response;

namespace sample.API.Endpoints.Todo.Queries;

[Route("api/todos/{id}")]
public class GetTodoByIdEndpoint(IMediator mediator) : EndpointBase<Request, Response>(mediator)
{
    [HttpGet]

    public override async Task<Shard.Responses.Response<Response>> HandleAsync(Request req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
