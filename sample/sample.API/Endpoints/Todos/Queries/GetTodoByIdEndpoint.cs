using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetById;
using Response = sample.Application.Features.Todo.Queries.GetById.Response;

namespace sample.API.Endpoints.Todos.Queries;

[OpenApiSummary("Get todo by ID", 
    Description = "Retrieves a specific todo item by its unique identifier",
    Tags = ["Todo Management", "Queries"])]
[OpenApiParameter("id", typeof(int), Description = "The unique identifier of the todo item", Required = true, Location = ParameterLocation.Path)]
[OpenApiResponse(200, ResponseType = typeof(Response<Response>), Description = "Todo item retrieved successfully")]
[OpenApiResponse(404, Description = "Todo item not found")]
public class GetTodoByIdEndpoint(IMediator mediator)
    : SingleEndpointBase<Request, Response>(mediator)
{
    [HttpGet("api/todos/{id}")]
    public override async Task<Response<Response>> HandleAsync(
        Request req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
