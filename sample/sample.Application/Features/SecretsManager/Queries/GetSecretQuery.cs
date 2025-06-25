using MediatR;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Queries;

/// <summary>
/// Query to retrieve a secret value from AWS Secrets Manager
/// </summary>
/// <param name="SecretName">The name or ARN of the secret to retrieve</param>
/// <param name="VersionId">Optional version ID of the secret</param>
/// <param name="VersionStage">Optional version stage of the secret</param>
public record GetSecretQuery(
    string SecretName,
    string? VersionId = null,
    string? VersionStage = null
) : IRequest<Response<GetSecretResponse>>;

/// <summary>
/// Response containing the secret value
/// </summary>
/// <param name="SecretName">The name of the secret</param>
/// <param name="SecretValue">The secret value</param>
/// <param name="VersionId">The version ID of the secret</param>
/// <param name="VersionStage">The version stage of the secret</param>
/// <param name="CreatedDate">The date when the secret was created</param>
/// <param name="LastAccessedDate">The date when the secret was last accessed</param>
public record GetSecretResponse(
    string SecretName,
    string SecretValue,
    string? VersionId,
    string? VersionStage,
    DateTime? CreatedDate,
    DateTime? LastAccessedDate
);
