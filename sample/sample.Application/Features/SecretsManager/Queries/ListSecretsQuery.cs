using MediatR;
using Shared.Responses;

namespace sample.Application.Features.SecretsManager.Queries;

/// <summary>
/// Query to list secrets in AWS Secrets Manager
/// </summary>
/// <param name="MaxResults">Maximum number of results to return (default: 100)</param>
/// <param name="NextToken">Token for pagination</param>
public record ListSecretsQuery(
    int MaxResults = 100,
    string? NextToken = null
) : IRequest<Response<ListSecretsResponse>>;

/// <summary>
/// Response containing list of secrets
/// </summary>
/// <param name="Secrets">List of secret summaries</param>
/// <param name="NextToken">Token for next page of results</param>
public record ListSecretsResponse(
    List<SecretSummary> Secrets,
    string? NextToken
);

/// <summary>
/// Summary information about a secret
/// </summary>
/// <param name="Name">Secret name</param>
/// <param name="ARN">Secret ARN</param>
/// <param name="Description">Secret description</param>
/// <param name="CreatedDate">Date created</param>
/// <param name="LastAccessedDate">Date last accessed</param>
/// <param name="LastChangedDate">Date last changed</param>
/// <param name="Tags">Secret tags</param>
public record SecretSummary(
    string Name,
    string ARN,
    string? Description,
    DateTime? CreatedDate,
    DateTime? LastAccessedDate,
    DateTime? LastChangedDate,
    Dictionary<string, string>? Tags
);
