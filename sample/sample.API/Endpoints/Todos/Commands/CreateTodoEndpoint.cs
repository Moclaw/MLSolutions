using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Commands.Create;

namespace sample.API.Endpoints.Todos.Commands;

[OpenApiSummary("Create a new todo", 
    Description = "Creates a new todo item with the provided details",
    Tags = ["Todo Management", "Commands"])]
[OpenApiResponse(201, ResponseType = typeof(Response<CreateResponse>), Description = "Todo created successfully")]
[OpenApiResponse(400, Description = "Invalid request data")]
public class CreateTodoEndpoint(IMediator mediator)
    : SingleEndpointBase<CreateRequest, CreateResponse>(mediator)
{
    [HttpPost("todos")]
    public override async Task<Response<CreateResponse>> HandleAsync(
        CreateRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
