using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.SecretsManager.Commands;
using Shared.Responses;

namespace sample.API.Endpoints.SecretsManager.Commands;

[OpenApiSummary("Create secret", 
    Description = "Creates a new secret in AWS Secrets Manager")]
[OpenApiResponse(201, ResponseType = typeof(Response<CreateSecretResponse>), Description = "Secret created successfully")]
[OpenApiResponse(409, Description = "Secret already exists")]
[ApiVersion("1.0")]
public class CreateSecretEndpoint(IMediator mediator)
    : SingleEndpointBase<CreateSecretCommand, CreateSecretResponse>(mediator)
{
    [HttpPost("secrets")]
    public override async Task<Response<CreateSecretResponse>> HandleAsync(
        CreateSecretCommand req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
