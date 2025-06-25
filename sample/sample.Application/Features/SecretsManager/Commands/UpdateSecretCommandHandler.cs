using MediatR;
using Microsoft.Extensions.Logging;
using Services.AWS.SecretsManager.Interfaces;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Commands;

/// <summary>
/// Handler for updating a secret in AWS Secrets Manager
/// </summary>
public class UpdateSecretCommandHandler(
    ISecretsManagerService secretsManagerService,
    ILogger<UpdateSecretCommandHandler> logger)
    : IRequestHandler<UpdateSecretCommand, Response<UpdateSecretResponse>>
{
    private readonly ISecretsManagerService _secretsManagerService = secretsManagerService;
    private readonly ILogger<UpdateSecretCommandHandler> _logger = logger;

    public async Task<Response<UpdateSecretResponse>> Handle(UpdateSecretCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating secret: {SecretName}", request.SecretName);

            // Check if secret exists
            var exists = await _secretsManagerService.SecretExistsAsync(request.SecretName);
            if (!exists)
            {
                _logger.LogWarning("Secret not found: {SecretName}", request.SecretName);
                return Response<UpdateSecretResponse>.Failure("Secret not found");
            }

            var result = await _secretsManagerService.UpdateSecretAsync(
                request.SecretName,
                request.SecretValue,
                request.Description);

            var response = new UpdateSecretResponse(
                ARN: result.ARN,
                Name: result.Name,
                VersionId: result.VersionId
            );

            _logger.LogInformation("Successfully updated secret: {SecretName} with new version: {VersionId}", 
                request.SecretName, result.VersionId);
            
            return Response<UpdateSecretResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating secret: {SecretName}", request.SecretName);
            return Response<UpdateSecretResponse>.Failure($"Error updating secret: {ex.Message}");
        }
    }
}
