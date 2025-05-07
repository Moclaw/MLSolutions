using Core.Configurations;
using Core.Constants;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MLSolutions;
using StackExchange.Redis;

namespace Services.Redis;

public class RedisCache : IRedisCache
{
    private readonly IDistributedCache _distributedCache;
    private const string CacheStoreKey = "CacheStore:Outbox:Keys";
    private readonly bool _isRedisCacheProvider;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCache(
        IDistributedCache distributedCache,
        IServiceProvider serviceProvider,
        IConnectionMultiplexer connectionMultiplexer
    )
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
        var configuration = serviceProvider.GetRequiredService<ModuleConfiguration>();
        _isRedisCacheProvider = configuration.Cache?.GetType() == typeof(RedisCacheDefintion);

        if (_isRedisCacheProvider)
        {
            _connectionMultiplexer = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default)
    {
        var cacheData = await _distributedCache.GetStringAsync(key, token: cancellation);

        if (string.IsNullOrEmpty(cacheData))
        {
            return default;
        }

        return cacheData.Deserialize<T>();
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> getDataFunction,
        int cacheTimeInMinutes,
        CancellationToken cancellation = default
    )
    {
        var value = await GetAsync<T?>(key, cancellation);
        if (!IsNullOrDefault(value))
        {
            return value;
        }

        value = await getDataFunction();

        if (!IsNullOrDefault(value))
        {
            await SetAsync(key, value, cacheTimeInMinutes, cancellation);
        }

        return value;
    }

    private static bool IsNullOrDefault<T>(T? value)
    {
        if (value is null)
        {
            return true;
        }

        return !typeof(T).IsValueTupleType()
            ? EqualityComparer<T>.Default.Equals(value, default)
            : typeof(T).GetFields().All(field => IsNullOrDefault(field.GetValue(value)));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellation = default)
    {
        await Task.WhenAll(
            SyncCacheKeyOutbox(key, true, cancellation),
            _distributedCache.RemoveAsync(key, cancellation)
        );
    }

    public async Task SetAsync<T>(
        string key,
        T data,
        int cacheTimeInMinutes,
        CancellationToken cancellation = default
    )
    {
        var serializedData = data.Serialize();

        await Task.WhenAll(
            SyncCacheKeyOutbox(key, false, cancellation),
            _distributedCache.SetStringAsync(
                key,
                serializedData,
                GetTimeOutOption(cacheTimeInMinutes),
                cancellation
            )
        );
    }

    private async Task SyncCacheKeyOutbox(
        string key,
        bool shouldRemove = false,
        CancellationToken cancellation = default
    )
    {
        if (_isRedisCacheProvider)
            return;

        var outboxKeys = (await GetAsync<HashSet<string>>(CacheStoreKey, cancellation)) ?? [];

        if (!shouldRemove && !outboxKeys.Add(key))
        {
            return;
        }

        if (shouldRemove && !outboxKeys.Remove(key))
        {
            return;
        }

        await _distributedCache.SetStringAsync(
            CacheStoreKey,
            outboxKeys.Serialize(),
            GetOutBoxCacheTimeOut(),
            cancellation
        );
    }

    private static DistributedCacheEntryOptions GetOutBoxCacheTimeOut()
    {
        return new DistributedCacheEntryOptions()
        {
            AbsoluteExpiration = DateTime.UtcNow.AddYears(20),
        };
    }

    public async Task<(bool, T? cacheData)> TryGetValueAsync<T>(
        string key,
        CancellationToken cancellation = default
    )
    {
        var cacheData = await GetAsync<T>(key, cancellation);

        return (cacheData != null, cacheData);
    }

    private static DistributedCacheEntryOptions GetTimeOutOption(int cacheTimeInMinutes)
    {
        DistributedCacheEntryOptions option = new();
        option.SetAbsoluteExpiration(DateTime.UtcNow.AddMinutes(cacheTimeInMinutes));

        return option;
    }

    public string GenerateKey(params string[] keys)
    {
        return string.Join(':', keys);
    }

    public async Task RemoveRangeAsync(
        string keyPattern,
        CacheKeySearchOperator searchOperator = CacheKeySearchOperator.StartsWith,
        CancellationToken cancellation = default
    )
    {
        if (!_isRedisCacheProvider)
        {
            await InMemoryRemoveRange(keyPattern, searchOperator, cancellation);

            return;
        }

        await RedisRemoveRange(keyPattern, searchOperator, cancellation);
    }

    private async Task RedisRemoveRange(
        string keyPattern,
        CacheKeySearchOperator searchOperator,
        CancellationToken cancellation
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyPattern, nameof(keyPattern));

        var server = _connectionMultiplexer.GetServer(
            _connectionMultiplexer.GetEndPoints().First()
        );
        var database = _connectionMultiplexer.GetDatabase();

        var searchPattern = searchOperator switch
        {
            CacheKeySearchOperator.StartsWith => $"{keyPattern}*",
            CacheKeySearchOperator.EndsWith => $"*{keyPattern}",
            CacheKeySearchOperator.Contains => $"*{keyPattern}*",
            _ => throw new ArgumentOutOfRangeException(
                nameof(searchOperator),
                searchOperator,
                null
            ),
        };

        var keys = server.Keys(database.Database, searchPattern);

        var redisKeys = keys as RedisKey[] ?? keys.ToArray();
        if (redisKeys.Length == 0)
        {
            return;
        }

        await database.KeyDeleteAsync([.. redisKeys]);
    }

    private async Task InMemoryRemoveRange(
        string keyPattern,
        CacheKeySearchOperator searchOperator,
        CancellationToken cancellation
    )
    {
        var outboxKeys = (await GetAsync<HashSet<string>>(CacheStoreKey, cancellation)) ?? [];

        if (outboxKeys.Count == 0)
        {
            return;
        }

        HashSet<string> keysToRemove = searchOperator switch
        {
            CacheKeySearchOperator.StartsWith =>
            [
                .. outboxKeys.Where(x => x.StartsWith(keyPattern)),
            ],
            CacheKeySearchOperator.EndsWith => [.. outboxKeys.Where(x => x.EndsWith(keyPattern))],
            CacheKeySearchOperator.Contains => [.. outboxKeys.Where(x => x.Contains(keyPattern))],
            _ => throw new ArgumentOutOfRangeException(
                nameof(searchOperator),
                searchOperator,
                null
            ),
        };

        if (keysToRemove.Count == 0)
        {
            return;
        }

        await Task.WhenAll(
            keysToRemove.Select(async key =>
            {
                await _distributedCache.RemoveAsync(key, cancellation);
            })
        );

        outboxKeys.RemoveWhere(x => keysToRemove.Contains(x));
        await _distributedCache.SetStringAsync(
            CacheStoreKey,
            outboxKeys.Serialize(),
            GetOutBoxCacheTimeOut(),
            cancellation
        );
    }
}
