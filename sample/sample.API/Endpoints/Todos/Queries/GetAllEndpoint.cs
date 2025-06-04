using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetAll;

namespace sample.API.Endpoints.Todos.Queries;

[OpenApiSummary("Get all todos", 
    Description = "Retrieves a paginated list of todos with optional search filtering")]
[OpenApiResponse(200, ResponseType = typeof(ResponseCollection<GetallResponse>), Description = "Successfully retrieved todos")]
[OpenApiResponse(400, Description = "Invalid request parameters")]
[ApiVersion("1.0")]

public class GetAllEndpoint(IMediator mediator)
    : CollectionEndpointBase<GetAllRequest, GetallResponse>(mediator)
{
    [HttpGet("todos")]
    public override async Task<ResponseCollection<GetallResponse>> HandleAsync(
        GetAllRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
