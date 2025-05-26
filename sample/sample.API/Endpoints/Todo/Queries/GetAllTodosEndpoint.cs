using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Queries.GetAll;
using Shared.Responses;
using EndpointSummaryAttribute = MinimalAPI.Attributes.EndpointSummaryAttribute;

namespace sample.API.Endpoints.Todo.Queries;

[EndpointSummary("Get all todos", Description = "Retrieves a paginated list of todos with optional search filtering")]
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
