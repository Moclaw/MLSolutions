using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Create;

namespace sample.API.Endpoints.Todo.Commands;

public class CreateTodoEndpoint(IMediator mediator) : EndpointBase<CreateRequest, CreateResponse>(mediator)
{
    [HttpPost("api/todos")]
    public override async Task<Shared.Responses.Response<CreateResponse>> HandleAsync(CreateRequest req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
