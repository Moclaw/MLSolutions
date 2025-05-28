using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.S3.Queries;
using Shared.Responses;

namespace sample.API.Endpoints.S3.Queries;

public class GetFileUrlEndpoint(IMediator mediator)
    : SingleEndpointBase<GetFileUrlQuery, GetFileUrlResponse>(mediator)
{
    [HttpGet("api/s3/url/{key}")]
    public override async Task<Response<GetFileUrlResponse>> HandleAsync(
        GetFileUrlQuery req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
