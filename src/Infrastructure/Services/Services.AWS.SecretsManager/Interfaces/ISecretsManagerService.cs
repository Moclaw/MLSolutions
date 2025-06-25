using Amazon.SecretsManager.Model;

namespace Services.AWS.SecretsManager.Interfaces;

/// <summary>
/// Interface for AWS Secrets Manager service operations
/// </summary>
public interface ISecretsManagerService
{
    /// <summary>
    /// Get a secret value by its name or ARN
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <param name="versionId">Optional version ID</param>
    /// <param name="versionStage">Optional version stage</param>
    /// <returns>Secret value as string</returns>
    Task<string> GetSecretValueAsync(string secretName, string? versionId = null, string? versionStage = null);

    /// <summary>
    /// Get a secret value as a specific type (JSON deserialization)
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="secretName">Secret name or ARN</param>
    /// <param name="versionId">Optional version ID</param>
    /// <param name="versionStage">Optional version stage</param>
    /// <returns>Deserialized secret value</returns>
    Task<T> GetSecretValueAsync<T>(string secretName, string? versionId = null, string? versionStage = null) where T : class;

    /// <summary>
    /// Create a new secret
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <param name="secretValue">Secret value</param>
    /// <param name="description">Optional description</param>
    /// <returns>Create secret response</returns>
    Task<CreateSecretResponse> CreateSecretAsync(string secretName, string secretValue, string? description = null);

    /// <summary>
    /// Update an existing secret value
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <param name="secretValue">New secret value</param>
    /// <param name="description">Optional description update</param>
    /// <returns>Update secret response</returns>
    Task<UpdateSecretResponse> UpdateSecretAsync(string secretName, string secretValue, string? description = null);

    /// <summary>
    /// Delete a secret (with optional recovery period)
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <param name="recoveryWindowInDays">Recovery window in days (7-30, default: 30)</param>
    /// <param name="forceDeleteWithoutRecovery">Force immediate deletion without recovery</param>
    /// <returns>Delete secret response</returns>
    Task<DeleteSecretResponse> DeleteSecretAsync(string secretName, int recoveryWindowInDays = 30, bool forceDeleteWithoutRecovery = false);

    /// <summary>
    /// Restore a previously deleted secret
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <returns>Restore secret response</returns>
    Task<RestoreSecretResponse> RestoreSecretAsync(string secretName);

    /// <summary>
    /// List all secrets in the account/region
    /// </summary>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="nextToken">Pagination token</param>
    /// <param name="filters">Optional filters</param>
    /// <returns>List of secret list entries</returns>
    Task<List<SecretListEntry>> ListSecretsAsync(int maxResults = 100, string? nextToken = null, List<Filter>? filters = null);

    /// <summary>
    /// Get metadata about a secret (without retrieving the secret value)
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <returns>Secret metadata</returns>
    Task<DescribeSecretResponse> GetSecretMetadataAsync(string secretName);

    /// <summary>
    /// Rotate a secret
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <param name="lambdaFunctionArn">Lambda function ARN for rotation</param>
    /// <param name="rotationRules">Rotation rules</param>
    /// <returns>Rotate secret response</returns>
    Task<RotateSecretResponse> RotateSecretAsync(string secretName, string lambdaFunctionArn, RotationRulesType? rotationRules = null);

    /// <summary>
    /// Add or update tags on a secret
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <param name="tags">Tags to add or update</param>
    /// <returns>Tag resource response</returns>
    Task<TagResourceResponse> TagSecretAsync(string secretName, List<Tag> tags);

    /// <summary>
    /// Remove tags from a secret
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <param name="tagKeys">Tag keys to remove</param>
    /// <returns>Untag resource response</returns>
    Task<UntagResourceResponse> UntagSecretAsync(string secretName, List<string> tagKeys);

    /// <summary>
    /// Check if a secret exists
    /// </summary>
    /// <param name="secretName">Secret name or ARN</param>
    /// <returns>True if secret exists, false otherwise</returns>
    Task<bool> SecretExistsAsync(string secretName);

    /// <summary>
    /// Get multiple secrets in a batch operation
    /// </summary>
    /// <param name="secretNames">List of secret names or ARNs</param>
    /// <returns>Dictionary of secret names and their values</returns>
    Task<Dictionary<string, string>> GetMultipleSecretsAsync(IEnumerable<string> secretNames);
}
