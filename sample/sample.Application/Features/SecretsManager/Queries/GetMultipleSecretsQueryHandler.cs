using MediatR;
using Microsoft.Extensions.Logging;
using Services.AWS.SecretsManager.Interfaces;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Queries;

/// <summary>
/// Handler for retrieving multiple secrets from AWS Secrets Manager
/// </summary>
public class GetMultipleSecretsQueryHandler(
    ISecretsManagerService secretsManagerService,
    ILogger<GetMultipleSecretsQueryHandler> logger)
    : IRequestHandler<GetMultipleSecretsQuery, Response<GetMultipleSecretsResponse>>
{
    private readonly ISecretsManagerService _secretsManagerService = secretsManagerService;
    private readonly ILogger<GetMultipleSecretsQueryHandler> _logger = logger;

    public async Task<Response<GetMultipleSecretsResponse>> Handle(GetMultipleSecretsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving {Count} secrets", request.SecretNames.Count());

            var secrets = await _secretsManagerService.GetMultipleSecretsAsync(request.SecretNames);

            var response = new GetMultipleSecretsResponse(secrets);

            _logger.LogInformation("Successfully retrieved {Count} secrets", secrets.Count);
            return Response<GetMultipleSecretsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multiple secrets");
            return Response<GetMultipleSecretsResponse>.Failure($"Error retrieving secrets: {ex.Message}");
        }
    }
}
