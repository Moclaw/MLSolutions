using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.Todo.Queries.GetAllTags;
using Response = sample.Application.Features.Todo.Queries.GetAllTags.Response;

namespace sample.API.Endpoints.Todo.Queries;
public class GetAllTagsEndpoint(IMediator mediator)
    : CollectionEndpointBase<Request, Response>(mediator)
{
    [HttpGet("api/tags")]
    public override async Task<ResponseCollection<Response>> HandleAsync(
        Request req,
        CancellationToken ct
    )
    {
        return await mediator.Send(req, ct);
    }
}