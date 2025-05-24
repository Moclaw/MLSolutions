using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Queries.GetAll;
using Shared.Responses;

namespace sample.API.Endpoints.Todo.Queries;

public class GetAllTodosEndpoint(IMediator mediator)
    : CollectionEndpointBase<GetAllRequest, GetallResponse>(mediator)
{
    [HttpGet("api/todos")]
    public override async Task<ResponesCollection<GetallResponse>> HandleAsync(
        GetAllRequest req,
        CancellationToken ct
    )
    {
        return await mediator.Send(req, ct);
    }
}
