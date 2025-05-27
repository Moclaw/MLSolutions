using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Commands.Create;

namespace sample.API.Endpoints.Todo.Commands;

[OpenApiSummary("Create a new todo", 
    Description = "Creates a new todo item with the provided details",
    Tags = ["Todo Management", "Commands"])]
[OpenApiResponse(201, ResponseType = typeof(Response<CreateResponse>), Description = "Todo created successfully")]
[OpenApiResponse(400, Description = "Invalid request data")]
public class CreateTodoEndpoint(IMediator mediator)
    : SingleEndpointBase<CreateRequest, CreateResponse>(mediator)
{
    [HttpPost("api/todos")]
    public override async Task<Shared.Responses.Response<CreateResponse>> HandleAsync(
        CreateRequest req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
