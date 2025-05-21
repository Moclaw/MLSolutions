using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Queries.GetAll;
using Shared.Responses;

namespace sample.API.Endpoints.Todo.Queries;

[Route("api/todos")]
public class GetAllTodosEndpoint(IMediator mediator) : EndpointBase<GetAllRequest, GetallResponse>(mediator)
{
    [HttpGet]
    public override async Task<Response<GetallResponse>> HandleAsync(GetAllRequest req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
