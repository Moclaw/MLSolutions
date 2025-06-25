using MediatR;
using Microsoft.Extensions.Logging;
using Services.AWS.SecretsManager.Interfaces;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Queries;

/// <summary>
/// Handler for listing secrets from AWS Secrets Manager
/// </summary>
public class ListSecretsQueryHandler(
    ISecretsManagerService secretsManagerService,
    ILogger<ListSecretsQueryHandler> logger)
    : IRequestHandler<ListSecretsQuery, Response<ListSecretsResponse>>
{
    private readonly ISecretsManagerService _secretsManagerService = secretsManagerService;
    private readonly ILogger<ListSecretsQueryHandler> _logger = logger;

    public async Task<Response<ListSecretsResponse>> Handle(ListSecretsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Listing secrets with MaxResults: {MaxResults}", request.MaxResults);

            var secretEntries = await _secretsManagerService.ListSecretsAsync(
                request.MaxResults, 
                request.NextToken);

            var secrets = secretEntries.Select(s => new SecretSummary(
                Name: s.Name,
                ARN: s.ARN,
                Description: s.Description,
                CreatedDate: s.CreatedDate,
                LastAccessedDate: s.LastAccessedDate,
                LastChangedDate: s.LastChangedDate,
                Tags: s.Tags?.ToDictionary(t => t.Key, t => t.Value)
            )).ToList();

            // Note: AWS SDK doesn't return NextToken in ListSecretsAsync response
            // In a real implementation, you might need to handle pagination differently
            var response = new ListSecretsResponse(secrets, null);

            _logger.LogInformation("Successfully listed {Count} secrets", secrets.Count);
            return Response<ListSecretsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing secrets");
            return Response<ListSecretsResponse>.Failure($"Error listing secrets: {ex.Message}");
        }
    }
}
