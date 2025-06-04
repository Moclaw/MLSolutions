using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.S3.Commands;
using Shared.Responses;

namespace sample.API.Endpoints.S3.Commands;

[OpenApiSummary("Delete S3 file", 
    Description = "Deletes a file from S3 storage")]
[OpenApiResponse(200, ResponseType = typeof(Response<DeleteFileResponse>), Description = "File deleted successfully")]
[OpenApiResponse(404, Description = "File not found")]
[ApiVersion("1.0")]
public class DeleteFileEndpoint(IMediator mediator)
    : SingleEndpointBase<DeleteFileCommand, DeleteFileResponse>(mediator)
{
    [HttpDelete("s3/{key}")]
    public override async Task<Response<DeleteFileResponse>> HandleAsync(
        DeleteFileCommand req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
