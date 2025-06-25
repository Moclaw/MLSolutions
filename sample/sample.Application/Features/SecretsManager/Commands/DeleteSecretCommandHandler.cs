using MediatR;
using Microsoft.Extensions.Logging;
using Services.AWS.SecretsManager.Interfaces;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Commands;

/// <summary>
/// Handler for deleting a secret from AWS Secrets Manager
/// </summary>
public class DeleteSecretCommandHandler(
    ISecretsManagerService secretsManagerService,
    ILogger<DeleteSecretCommandHandler> logger)
    : IRequestHandler<DeleteSecretCommand, Response<DeleteSecretResponse>>
{
    private readonly ISecretsManagerService _secretsManagerService = secretsManagerService;
    private readonly ILogger<DeleteSecretCommandHandler> _logger = logger;

    public async Task<Response<DeleteSecretResponse>> Handle(DeleteSecretCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting secret: {SecretName}", request.SecretName);

            // Check if secret exists
            var exists = await _secretsManagerService.SecretExistsAsync(request.SecretName);
            if (!exists)
            {
                _logger.LogWarning("Secret not found: {SecretName}", request.SecretName);
                return Response<DeleteSecretResponse>.Failure("Secret not found");
            }

            var result = await _secretsManagerService.DeleteSecretAsync(
                request.SecretName,
                request.RecoveryWindowInDays,
                request.ForceDeleteWithoutRecovery);

            var response = new DeleteSecretResponse(
                ARN: result.ARN,
                Name: result.Name,
                DeletionDate: result.DeletionDate
            );

            _logger.LogInformation("Successfully scheduled secret deletion: {SecretName} on {DeletionDate}", 
                request.SecretName, result.DeletionDate);
            
            return Response<DeleteSecretResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secret: {SecretName}", request.SecretName);
            return Response<DeleteSecretResponse>.Failure($"Error deleting secret: {ex.Message}");
        }
    }
}
