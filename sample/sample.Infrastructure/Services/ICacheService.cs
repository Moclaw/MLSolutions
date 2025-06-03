using Microsoft.Extensions.Logging;
using Services.Autofac.Attributes;

namespace sample.Infrastructure.Services;

/// <summary>
/// Service for caching operations - demonstrates named service registration
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task ClearAsync();
}

/// <summary>
/// In-memory cache implementation
/// </summary>
[SingletonService(ServiceName = "MemoryCache")]
public class MemoryCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime Expiration)> _cache = new();
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(ILogger<MemoryCacheService> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _logger.LogDebug("Getting cache value for key: {Key}", key);
        
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.Expiration > DateTime.UtcNow)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult((T?)item.Value);
            }
            else
            {
                _logger.LogDebug("Cache expired for key: {Key}", key);
                _cache.Remove(key);
            }
        }
        
        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var exp = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(30));
        _cache[key] = (value!, exp);
        _logger.LogDebug("Cached value for key: {Key}, expires: {Expiration}", key, exp);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Removed cache entry for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _cache.Clear();
        _logger.LogInformation("Cleared all cache entries");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Redis cache implementation (mock for demonstration)
/// </summary>
[SingletonService(ServiceName = "RedisCache")]
public class RedisCacheService : ICacheService
{
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(ILogger<RedisCacheService> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _logger.LogDebug("Redis: Getting cache value for key: {Key}", key);
        // Mock implementation - would connect to Redis in real scenario
        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _logger.LogDebug("Redis: Setting cache value for key: {Key}", key);
        // Mock implementation - would connect to Redis in real scenario
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _logger.LogDebug("Redis: Removing cache entry for key: {Key}", key);
        // Mock implementation - would connect to Redis in real scenario
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _logger.LogInformation("Redis: Clearing all cache entries");
        // Mock implementation - would connect to Redis in real scenario
        return Task.CompletedTask;
    }
}
