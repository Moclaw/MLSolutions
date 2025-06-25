using MediatR;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Commands;

/// <summary>
/// Command to delete a secret from AWS Secrets Manager
/// </summary>
/// <param name="SecretName">Name or ARN of the secret to delete</param>
/// <param name="RecoveryWindowInDays">Recovery window in days (7-30, default: 30)</param>
/// <param name="ForceDeleteWithoutRecovery">Force immediate deletion without recovery</param>
public record DeleteSecretCommand(
    string SecretName,
    int RecoveryWindowInDays = 30,
    bool ForceDeleteWithoutRecovery = false
) : IRequest<Response<DeleteSecretResponse>>;

/// <summary>
/// Response for deleting a secret
/// </summary>
/// <param name="ARN">ARN of the deleted secret</param>
/// <param name="Name">Name of the deleted secret</param>
/// <param name="DeletionDate">Scheduled deletion date</param>
public record DeleteSecretResponse(
    string ARN,
    string Name,
    DateTime? DeletionDate
);
