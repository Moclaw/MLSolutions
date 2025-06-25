using MediatR;
using Microsoft.Extensions.Logging;
using Services.AWS.SecretsManager.Interfaces;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Queries;

/// <summary>
/// Handler for retrieving a secret from AWS Secrets Manager
/// </summary>
public class GetSecretQueryHandler(
    ISecretsManagerService secretsManagerService,
    ILogger<GetSecretQueryHandler> logger)
    : IRequestHandler<GetSecretQuery, Response<GetSecretResponse>>
{
    private readonly ISecretsManagerService _secretsManagerService = secretsManagerService;
    private readonly ILogger<GetSecretQueryHandler> _logger = logger;

    public async Task<Response<GetSecretResponse>> Handle(GetSecretQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving secret: {SecretName}", request.SecretName);

            // Check if secret exists first
            var exists = await _secretsManagerService.SecretExistsAsync(request.SecretName);
            if (!exists)
            {
                _logger.LogWarning("Secret not found: {SecretName}", request.SecretName);
                return Response<GetSecretResponse>.Failure("Secret not found");
            }

            // Get secret value
            var secretValue = await _secretsManagerService.GetSecretValueAsync(
                request.SecretName, 
                request.VersionId, 
                request.VersionStage);

            // Get secret metadata
            var metadata = await _secretsManagerService.GetSecretMetadataAsync(request.SecretName);

            var response = new GetSecretResponse(
                SecretName: request.SecretName,
                SecretValue: secretValue,
                VersionId: request.VersionId,
                VersionStage: request.VersionStage,
                CreatedDate: metadata.CreatedDate,
                LastAccessedDate: metadata.LastAccessedDate
            );

            _logger.LogInformation("Successfully retrieved secret: {SecretName}", request.SecretName);
            return Response<GetSecretResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret: {SecretName}", request.SecretName);
            return Response<GetSecretResponse>.Failure($"Error retrieving secret: {ex.Message}");
        }
    }
}
