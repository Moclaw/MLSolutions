using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.S3.Commands;
using Shared.Responses;

namespace sample.API.Endpoints.S3.Commands;

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
