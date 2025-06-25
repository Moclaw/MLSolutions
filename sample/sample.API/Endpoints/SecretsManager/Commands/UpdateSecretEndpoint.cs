using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.SecretsManager.Commands;
using Shared.Responses;

namespace sample.API.Endpoints.SecretsManager.Commands;

[OpenApiSummary("Update secret", 
    Description = "Updates an existing secret in AWS Secrets Manager")]
[OpenApiResponse(200, ResponseType = typeof(Response<UpdateSecretResponse>), Description = "Secret updated successfully")]
[OpenApiResponse(404, Description = "Secret not found")]
[ApiVersion("1.0")]
public class UpdateSecretEndpoint(IMediator mediator)
    : SingleEndpointBase<UpdateSecretCommand, UpdateSecretResponse>(mediator)
{
    [HttpPut("secrets/{secretName}")]
    public override async Task<Response<UpdateSecretResponse>> HandleAsync(
        UpdateSecretCommand req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
