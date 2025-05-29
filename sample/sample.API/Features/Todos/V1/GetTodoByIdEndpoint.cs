using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetById;

namespace sample.API.Features.Todos.V1;

[OpenApiSummary("Get a todo item by ID", 
    Description = "Retrieves a specific todo item by its unique identifier (Version 1.0)",
    Tags = new[] { "Todo Management v1.0" })]
[OpenApiParameter("id", typeof(int), Description = "The unique identifier of the todo item", Required = true)]
public class GetTodoByIdEndpoint(IMediator mediator)
    : SingleEndpointBase<GetByIdRequest, GetByIdResponse>(mediator)
{
    [HttpGet("todos/{id}")]
    public override async Task<Response<GetByIdResponse>> HandleAsync(
        GetByIdRequest req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
