using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Commands.Create;

namespace sample.API.Features.Todos.V1;

[OpenApiSummary("Create a new todo item (Deprecated)", 
    Description = "Creates a new todo item with basic information. This version is deprecated, please use v2.0",
    Tags = new[] { "Todo Management v1.0 (Deprecated)" })]
public class CreateTodoEndpoint(IMediator mediator)
    : SingleEndpointBase<CreateRequest, CreateResponse>(mediator)
{
    [HttpPost("todos")]
    public override async Task<Response<CreateResponse>> HandleAsync(
        CreateRequest req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
