using MediatR;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Queries;

/// <summary>
/// Query to retrieve multiple secrets from AWS Secrets Manager
/// </summary>
/// <param name="SecretNames">List of secret names or ARNs to retrieve</param>
public record GetMultipleSecretsQuery(
    IEnumerable<string> SecretNames
) : IRequest<Response<GetMultipleSecretsResponse>>;

/// <summary>
/// Response containing multiple secret values
/// </summary>
/// <param name="Secrets">Dictionary of secret names and their values</param>
public record GetMultipleSecretsResponse(
    Dictionary<string, string> Secrets
);
