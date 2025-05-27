using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetAll;

namespace sample.API.Endpoints.Todo.Queries;

[OpenApiSummary("Get all todos", 
    Description = "Retrieves a paginated list of todos with optional search filtering",
    Tags = ["Todo Management", "Queries"])]
[OpenApiResponse(200, ResponseType = typeof(ResponseCollection<GetallResponse>), Description = "Successfully retrieved todos")]
[OpenApiResponse(400, Description = "Invalid request parameters")]
public class GetAllTodosEndpoint(IMediator mediator)
    : CollectionEndpointBase<GetAllRequest, GetallResponse>(mediator)
{
    [HttpGet("api/todos")]
    public override async Task<ResponseCollection<GetallResponse>> HandleAsync(
        GetAllRequest req,
        CancellationToken ct
    )
    {
        return await mediator.Send(req, ct);
    }
}
