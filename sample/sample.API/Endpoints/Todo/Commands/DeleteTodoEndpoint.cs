using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Delete;

namespace sample.API.Endpoints.Todo.Commands;

public class DeleteTodoEndpoint(IMediator mediator) : SingleEndpointBase<Request>(mediator)
{
    [HttpDelete("api/todos/{id}")]
    public override async Task<Response> HandleAsync(Request req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
