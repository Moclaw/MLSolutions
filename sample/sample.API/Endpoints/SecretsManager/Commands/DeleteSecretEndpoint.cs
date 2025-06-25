using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.SecretsManager.Commands;
using Shared.Responses;

namespace sample.API.Endpoints.SecretsManager.Commands;

[OpenApiSummary("Delete secret", 
    Description = "Deletes a secret from AWS Secrets Manager")]
[OpenApiResponse(200, ResponseType = typeof(Response<DeleteSecretResponse>), Description = "Secret deleted successfully")]
[OpenApiResponse(404, Description = "Secret not found")]
[ApiVersion("1.0")]
public class DeleteSecretEndpoint(IMediator mediator)
    : SingleEndpointBase<DeleteSecretCommand, DeleteSecretResponse>(mediator)
{
    [HttpDelete("secrets/{secretName}")]
    public override async Task<Response<DeleteSecretResponse>> HandleAsync(
        DeleteSecretCommand req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
