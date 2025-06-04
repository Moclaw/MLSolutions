using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Commands.Delete;

namespace sample.API.Endpoints.Todos.Commands;

[OpenApiSummary("Delete a todo", 
    Description = "Permanently deletes a todo item from the system")]
[OpenApiResponse(200, Description = "Todo deleted successfully")]
[OpenApiResponse(404, Description = "Todo item not found")]
[ApiVersion("1.0")]

public class DeleteTodoEndpoint(IMediator mediator) : SingleEndpointBase<Request>(mediator)
{
    [HttpDelete("todos/{id}")]
    public override async Task<Response> HandleAsync(Request req, CancellationToken ct) => await _mediator.Send(req, ct);
}
