using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.S3.Commands;
using Shared.Responses;

namespace sample.API.Endpoints.S3.Commands;

public class UploadFileEndpoint(IMediator mediator)
    : SingleEndpointBase<UploadFileCommand, UploadFileResponse>(mediator)
{
    [HttpPost("s3/upload")]
    public override async Task<Response<UploadFileResponse>> HandleAsync(
        UploadFileCommand req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
