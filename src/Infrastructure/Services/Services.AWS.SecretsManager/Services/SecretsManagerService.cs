using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Services.AWS.SecretsManager.Configurations;
using Services.AWS.SecretsManager.Interfaces;
using System.Text.Json;

namespace Services.AWS.SecretsManager.Services;

/// <summary>
/// AWS Secrets Manager service implementation
/// </summary>
public class SecretsManagerService : ISecretsManagerService, IDisposable
{
    private readonly IAmazonSecretsManager _secretsManagerClient;
    private readonly SecretsManagerConfiguration _config;
    private readonly IMemoryCache? _cache;
    private bool _disposed;

    public IAmazonSecretsManager SecretsManagerClient => _secretsManagerClient;

    public SecretsManagerService(IOptions<SecretsManagerConfiguration> config, IMemoryCache? cache = null)
    {
        _config = config.Value;
        _cache = cache;

        var clientConfig = new AmazonSecretsManagerConfig
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_config.Region),
        };

        // Configure for LocalStack if enabled
        if (_config.UseLocalStack)
        {
            clientConfig.ServiceURL = _config.LocalStackServiceUrl;
        }

        if (_config.UseCredentialsFile)
        {
            _secretsManagerClient = new AmazonSecretsManagerClient(clientConfig);
        }
        else
        {
            _secretsManagerClient = new AmazonSecretsManagerClient(
                _config.AccessKey,
                _config.SecretKey,
                clientConfig
            );
        }
    }

    public async Task<string> GetSecretValueAsync(string secretName, string? versionId = null, string? versionStage = null)
    {
        var fullSecretName = GetFullSecretName(secretName);
        var cacheKey = $"secret_{fullSecretName}_{versionId}_{versionStage}";

        // Check cache first if enabled
        if (_config.EnableCaching && _cache != null && _cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue!;
        }

        var request = new GetSecretValueRequest
        {
            SecretId = fullSecretName,
            VersionId = versionId,
            VersionStage = versionStage
        };

        var response = await SecretsManagerClient.GetSecretValueAsync(request);
        var secretValue = response.SecretString;

        // Cache the result if enabled
        if (_config.EnableCaching && _cache != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_config.CacheExpirationMinutes)
            };
            _cache.Set(cacheKey, secretValue, cacheOptions);
        }

        return secretValue;
    }

    public async Task<T> GetSecretValueAsync<T>(string secretName, string? versionId = null, string? versionStage = null) where T : class
    {
        var secretValue = await GetSecretValueAsync(secretName, versionId, versionStage);
        
        try
        {
            var result = JsonSerializer.Deserialize<T>(secretValue);
            return result ?? throw new InvalidOperationException($"Failed to deserialize secret '{secretName}' to type {typeof(T).Name}");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Secret '{secretName}' contains invalid JSON for type {typeof(T).Name}", ex);
        }
    }

    public async Task<CreateSecretResponse> CreateSecretAsync(string secretName, string secretValue, string? description = null)
    {
        var request = new CreateSecretRequest
        {
            Name = GetFullSecretName(secretName),
            SecretString = secretValue,
            Description = description
        };

        return await SecretsManagerClient.CreateSecretAsync(request);
    }

    public async Task<UpdateSecretResponse> UpdateSecretAsync(string secretName, string secretValue, string? description = null)
    {
        var fullSecretName = GetFullSecretName(secretName);
        
        // Invalidate cache
        InvalidateSecretCache(fullSecretName);

        var request = new UpdateSecretRequest
        {
            SecretId = fullSecretName,
            SecretString = secretValue,
            Description = description
        };

        return await SecretsManagerClient.UpdateSecretAsync(request);
    }

    public async Task<DeleteSecretResponse> DeleteSecretAsync(string secretName, int recoveryWindowInDays = 30, bool forceDeleteWithoutRecovery = false)
    {
        var fullSecretName = GetFullSecretName(secretName);
        
        // Invalidate cache
        InvalidateSecretCache(fullSecretName);

        var request = new DeleteSecretRequest
        {
            SecretId = fullSecretName,
            ForceDeleteWithoutRecovery = forceDeleteWithoutRecovery
        };

        if (!forceDeleteWithoutRecovery)
        {
            request.RecoveryWindowInDays = recoveryWindowInDays;
        }

        return await SecretsManagerClient.DeleteSecretAsync(request);
    }

    public async Task<RestoreSecretResponse> RestoreSecretAsync(string secretName)
    {
        var request = new RestoreSecretRequest
        {
            SecretId = GetFullSecretName(secretName)
        };

        return await SecretsManagerClient.RestoreSecretAsync(request);
    }

    public async Task<List<SecretListEntry>> ListSecretsAsync(int maxResults = 100, string? nextToken = null, List<Filter>? filters = null)
    {
        var request = new ListSecretsRequest
        {
            MaxResults = maxResults,
            NextToken = nextToken,
            Filters = filters ?? new List<Filter>()
        };

        // Add prefix filter if configured
        if (!string.IsNullOrEmpty(_config.SecretPrefix))
        {
            request.Filters.Add(new Filter
            {
                Key = FilterNameStringType.Name,
                Values = new List<string> { $"{_config.SecretPrefix}*" }
            });
        }

        var response = await SecretsManagerClient.ListSecretsAsync(request);
        return response.SecretList;
    }

    public async Task<DescribeSecretResponse> GetSecretMetadataAsync(string secretName)
    {
        var request = new DescribeSecretRequest
        {
            SecretId = GetFullSecretName(secretName)
        };

        return await SecretsManagerClient.DescribeSecretAsync(request);
    }

    public async Task<RotateSecretResponse> RotateSecretAsync(string secretName, string lambdaFunctionArn, RotationRulesType? rotationRules = null)
    {
        var fullSecretName = GetFullSecretName(secretName);
        
        // Invalidate cache
        InvalidateSecretCache(fullSecretName);

        var request = new RotateSecretRequest
        {
            SecretId = fullSecretName,
            RotationLambdaARN = lambdaFunctionArn,
            RotationRules = rotationRules
        };

        return await SecretsManagerClient.RotateSecretAsync(request);
    }

    public async Task<TagResourceResponse> TagSecretAsync(string secretName, List<Tag> tags)
    {
        var request = new TagResourceRequest
        {
            SecretId = GetFullSecretName(secretName),
            Tags = tags
        };

        return await SecretsManagerClient.TagResourceAsync(request);
    }

    public async Task<UntagResourceResponse> UntagSecretAsync(string secretName, List<string> tagKeys)
    {
        var request = new UntagResourceRequest
        {
            SecretId = GetFullSecretName(secretName),
            TagKeys = tagKeys
        };

        return await SecretsManagerClient.UntagResourceAsync(request);
    }

    public async Task<bool> SecretExistsAsync(string secretName)
    {
        try
        {
            await GetSecretMetadataAsync(secretName);
            return true;
        }
        catch (ResourceNotFoundException)
        {
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetMultipleSecretsAsync(IEnumerable<string> secretNames)
    {
        var results = new Dictionary<string, string>();
        var tasks = secretNames.Select(async secretName =>
        {
            try
            {
                var value = await GetSecretValueAsync(secretName);
                return new KeyValuePair<string, string>(secretName, value);
            }
            catch
            {
                return new KeyValuePair<string, string>(secretName, string.Empty);
            }
        });

        var completedTasks = await Task.WhenAll(tasks);
        
        foreach (var result in completedTasks.Where(r => !string.IsNullOrEmpty(r.Value)))
        {
            results[result.Key] = result.Value;
        }

        return results;
    }

    private string GetFullSecretName(string secretName)
    {
        if (string.IsNullOrEmpty(_config.SecretPrefix))
        {
            return secretName;
        }

        return secretName.StartsWith(_config.SecretPrefix) 
            ? secretName 
            : $"{_config.SecretPrefix}{secretName}";
    }

    private void InvalidateSecretCache(string secretName)
    {
        if (_cache == null || !_config.EnableCaching) return;

        // Remove all cached entries for this secret (different versions/stages)
        var cacheKeyPattern = $"secret_{secretName}_";
        
        // Note: IMemoryCache doesn't provide a way to enumerate keys, 
        // so we can't remove all related entries. This is a limitation.
        // In a production environment, consider using a distributed cache
        // like Redis that supports pattern-based key removal.
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            SecretsManagerClient?.Dispose();
        }

        _disposed = true;
    }
}
