using MediatR;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Commands;

/// <summary>
/// Command to create a new secret in AWS Secrets Manager
/// </summary>
/// <param name="SecretName">Name of the secret to create</param>
/// <param name="SecretValue">Value of the secret</param>
/// <param name="Description">Optional description for the secret</param>
public record CreateSecretCommand(
    string SecretName,
    string SecretValue,
    string? Description = null
) : IRequest<Response<CreateSecretResponse>>;

/// <summary>
/// Response for creating a secret
/// </summary>
/// <param name="ARN">ARN of the created secret</param>
/// <param name="Name">Name of the created secret</param>
/// <param name="VersionId">Version ID of the created secret</param>
public record CreateSecretResponse(
    string ARN,
    string Name,
    string VersionId
);
