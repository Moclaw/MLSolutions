using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Delete;

namespace sample.API.Endpoints.Todo.Commands;

[Route("api/todos/{id}")]
public class DeleteTodoEndpoint(IMediator mediator) : EndpointBase<Request>(mediator)
{
    [HttpDelete]
    public override async Task<Response> HandleAsync(Request req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
