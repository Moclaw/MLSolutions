using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetById;
using GetByIdResponse = sample.Application.Features.Todo.Queries.GetById.GetByIdResponse;

namespace sample.API.Endpoints.Todos.Queries;

[OpenApiSummary("Get todo by ID", 
    Description = "Retrieves a specific todo item by its unique identifier")]
[ApiVersion(2)]
public class GetTodoByIdEndpoint(IMediator mediator)
    : SingleEndpointBase<GetByIdRequest, GetByIdResponse>(mediator)
{
    [HttpGet("todos/{id}")]
    public override async Task<Response<GetByIdResponse>> HandleAsync(
        GetByIdRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
