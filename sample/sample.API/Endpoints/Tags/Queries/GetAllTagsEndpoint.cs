using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.Todo.Queries.GetAllTags;
using Response = sample.Application.Features.Todo.Queries.GetAllTags.Response;

namespace sample.API.Endpoints.Tags.Queries;

[OpenApiSummary("Get all tags", 
    Description = "Retrieves a paginated list of all available tags",
    Tags = ["Tag Management", "Queries"])]
[OpenApiResponse(200, ResponseType = typeof(ResponseCollection<Response>), Description = "Successfully retrieved tags")]
[OpenApiResponse(400, Description = "Invalid request parameters")]
public class GetAllTagsEndpoint(IMediator mediator)
    : CollectionEndpointBase<Request, Response>(mediator)
{
    [HttpGet("api/tags")]
    public override async Task<ResponseCollection<Response>> HandleAsync(
        Request req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}