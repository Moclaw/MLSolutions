using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Create;

namespace sample.API.Endpoints.Todo.Commands;

[Route("api/todos")]
public class CreateTodoEndpoint(IMediator mediator) : EndpointBase<CreateRequest, CreateResponse>(mediator)
{
    public override async Task<Shard.Responses.Response<CreateResponse>> HandleAsync(CreateRequest req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
