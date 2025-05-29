using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.S3.Queries;
using Shared.Responses;

namespace sample.API.Endpoints.S3.Queries;

[OpenApiSummary("Get file URL", 
    Description = "Retrieves a presigned URL for accessing a file in S3 storage",
    Tags = ["S3 Management", "Queries"])]
[OpenApiParameter("key", typeof(string), Description = "The S3 object key", Required = true, Location = ParameterLocation.Path)]
[ApiVersion("1.0")]
public class GetFileUrlEndpoint(IMediator mediator)
    : SingleEndpointBase<GetFileUrlQuery, GetFileUrlResponse>(mediator)
{
    [HttpGet("s3/url/{key}")]
    public override async Task<Response<GetFileUrlResponse>> HandleAsync(
        GetFileUrlQuery req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
