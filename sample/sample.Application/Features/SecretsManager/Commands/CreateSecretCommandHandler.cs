using MediatR;
using Microsoft.Extensions.Logging;
using Services.AWS.SecretsManager.Interfaces;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Commands;

/// <summary>
/// Handler for creating a secret in AWS Secrets Manager
/// </summary>
public class CreateSecretCommandHandler(
    ISecretsManagerService secretsManagerService,
    ILogger<CreateSecretCommandHandler> logger)
    : IRequestHandler<CreateSecretCommand, Response<CreateSecretResponse>>
{
    private readonly ISecretsManagerService _secretsManagerService = secretsManagerService;
    private readonly ILogger<CreateSecretCommandHandler> _logger = logger;

    public async Task<Response<CreateSecretResponse>> Handle(CreateSecretCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating secret: {SecretName}", request.SecretName);

            // Check if secret already exists
            var exists = await _secretsManagerService.SecretExistsAsync(request.SecretName);
            if (exists)
            {
                _logger.LogWarning("Secret already exists: {SecretName}", request.SecretName);
                return Response<CreateSecretResponse>.Failure("Secret already exists");
            }

            var result = await _secretsManagerService.CreateSecretAsync(
                request.SecretName,
                request.SecretValue,
                request.Description);

            var response = new CreateSecretResponse(
                ARN: result.ARN,
                Name: result.Name,
                VersionId: result.VersionId
            );

            _logger.LogInformation("Successfully created secret: {SecretName} with ARN: {ARN}", 
                request.SecretName, result.ARN);
            
            return Response<CreateSecretResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secret: {SecretName}", request.SecretName);
            return Response<CreateSecretResponse>.Failure($"Error creating secret: {ex.Message}");
        }
    }
}
