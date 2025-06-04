using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.S3.Commands;
using Shared.Responses;

namespace sample.API.Endpoints.S3.Commands;

[OpenApiSummary("Upload file to S3", 
    Description = "Uploads a file to S3 storage")]
[OpenApiResponse(200, ResponseType = typeof(Response<UploadFileResponse>), Description = "File uploaded successfully")]
[OpenApiResponse(400, Description = "Invalid file or upload failed")]
[ApiVersion("1.0")]
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
