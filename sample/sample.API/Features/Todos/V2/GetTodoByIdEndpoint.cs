using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetById;

namespace sample.API.Features.Todos.V2;

[OpenApiSummary("Get a todo item by ID with enhanced data", 
    Description = "Retrieves a specific todo item by its unique identifier with additional metadata and related data (Version 2.0)",
    Tags = new[] { "Todo Management v2.0" })]
[OpenApiParameter("id", typeof(int), Description = "The unique identifier of the todo item", Required = true)]
[OpenApiParameter("includeCategory", typeof(bool), Description = "Include category information", Required = false)]
[OpenApiParameter("includeTags", typeof(bool), Description = "Include tags information", Required = false)]
[OpenApiResponse(404, Description = "Todo item not found")]
public class GetTodoByIdEndpoint(IMediator mediator)
    : SingleEndpointBase<GetByIdRequest, GetByIdResponse>(mediator)
{
    [HttpGet("todos/{id}")]
    public override async Task<Response<GetByIdResponse>> HandleAsync(
        GetByIdRequest req,
        CancellationToken ct
    )
    {
        // V2 can include additional logic for enhanced data
        return await _mediator.Send(req, ct);
    }
}
