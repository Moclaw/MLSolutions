using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.S3.Queries;
using Shared.Responses;

namespace sample.API.Endpoints.S3.Queries;

[OpenApiSummary("List S3 files", 
    Description = "Retrieves a list of files from S3 storage",
    Tags = ["S3 Management", "Queries"])]
[ApiVersion("1.0")]
public class ListFilesEndpoint(IMediator mediator)
    : SingleEndpointBase<ListFilesQuery, ListFilesResponse>(mediator)
{
    [HttpGet("s3/files")]
    public override async Task<Response<ListFilesResponse>> HandleAsync(
        ListFilesQuery req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
