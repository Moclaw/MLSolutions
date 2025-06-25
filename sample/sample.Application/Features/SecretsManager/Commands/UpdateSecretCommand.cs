using MediatR;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Commands;

/// <summary>
/// Command to update an existing secret in AWS Secrets Manager
/// </summary>
/// <param name="SecretName">Name or ARN of the secret to update</param>
/// <param name="SecretValue">New value for the secret</param>
/// <param name="Description">Optional description update</param>
public record UpdateSecretCommand(
    string SecretName,
    string SecretValue,
    string? Description = null
) : IRequest<Response<UpdateSecretResponse>>;

/// <summary>
/// Response for updating a secret
/// </summary>
/// <param name="ARN">ARN of the updated secret</param>
/// <param name="Name">Name of the updated secret</param>
/// <param name="VersionId">New version ID of the updated secret</param>
public record UpdateSecretResponse(
    string ARN,
    string Name,
    string VersionId
);
