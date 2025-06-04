using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetAllTags;
using GetAllTagsResponse = sample.Application.Features.Todo.Queries.GetAllTags.GetAllTagsResponse;

namespace sample.API.Endpoints.Tags.Queries;

[OpenApiSummary("Get all tags", 
    Description = "Retrieves a paginated list of all available tags")]
[ApiVersion("1.0")]
public class GetAllEndpoint(IMediator mediator)
    : CollectionEndpointBase<GetAllTagsRequest, GetAllTagsResponse>(mediator)
{
    [HttpGet("tags")]
    public override async Task<ResponseCollection<GetAllTagsResponse>> HandleAsync(
        GetAllTagsRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}