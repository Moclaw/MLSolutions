using Core.Constants;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MLSolutions;
using StackExchange.Redis;


namespace Services.Caching.Redis
{
    internal class RedisServices : IRedisServices
    {
        private readonly IDistributedCache _distributedCache;
        private const string CacheStoreKey = "CacheStore:Outbox:Keys";
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisServices(
            IDistributedCache distributedCache,
            IServiceProvider serviceProvider,
            IConnectionMultiplexer connectionMultiplexer
        )
        {
            _distributedCache = distributedCache;
            _connectionMultiplexer = connectionMultiplexer;
            _connectionMultiplexer = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        }

        public async Task<T?> GetCacheValueAsync<T>(string key, CancellationToken cancellation = default)
        {
            var cacheData = await _distributedCache.GetStringAsync(key, token: cancellation);
            return string.IsNullOrEmpty(cacheData) ? default : cacheData.Deserialize<T>();
        }

        public async Task<T?> GetOrSetCacheValueAsync<T>(
            string key,
            Func<Task<T>> valueFactory,
            int cacheTimeInMinutes,
            CancellationToken cancellation = default
        )
        {
            var value = await GetCacheValueAsync<T?>(key, cancellation);
            if (!IsNullOrDefault(value))
            {
                return value;
            }

            value = await valueFactory();

            if (!IsNullOrDefault(value))
            {
                await SetCacheValueAsync(key, value, cacheTimeInMinutes, cancellation);
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

        public async Task RemoveCacheValueAsync(string key, CancellationToken cancellation = default) => await Task.WhenAll(
                SyncCacheKeyOutbox(key, true, cancellation),
                _distributedCache.RemoveAsync(key, cancellation)
            );

        public async Task SetCacheValueAsync<T>(
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
            var outboxKeys = await GetCacheValueAsync<HashSet<string>>(CacheStoreKey, cancellation) ?? [];

            switch (shouldRemove)
            {
                case false when !outboxKeys.Add(key):
                case true when !outboxKeys.Remove(key):
                    return;
                default:
                    await _distributedCache.SetStringAsync(
                        CacheStoreKey,
                        outboxKeys.Serialize(),
                        GetOutBoxCacheTimeOut(),
                        cancellation
                    );
                    break;
            }
        }

        private static DistributedCacheEntryOptions GetOutBoxCacheTimeOut() => new DistributedCacheEntryOptions()
        {
            AbsoluteExpiration = DateTime.UtcNow.AddYears(20),
        };

        public async Task<(bool found, T? cacheData)> TryGetCacheValueAsync<T>(
            string key,
            CancellationToken cancellation = default
        )
        {
            var cacheData = await GetCacheValueAsync<T>(key, cancellation);
            return (cacheData != null, cacheData);
        }

        private static DistributedCacheEntryOptions GetTimeOutOption(int cacheTimeInMinutes)
        {
            DistributedCacheEntryOptions option = new();
            option.SetAbsoluteExpiration(DateTime.UtcNow.AddMinutes(cacheTimeInMinutes));
            return option;
        }

        public string BuildCacheKey(params string[] keys) => string.Join(':', keys);

        public async Task RemoveCacheRangeAsync(
            string keyPattern,
            CacheKeySearchOperator searchOperator = CacheKeySearchOperator.StartsWith,
            CancellationToken cancellation = default
        ) => await RedisRemoveRange(keyPattern, searchOperator);

        private async Task RedisRemoveRange(
            string keyPattern,
            CacheKeySearchOperator searchOperator
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

            var redisKeys = keys as RedisKey[] ?? [.. keys];
            if (redisKeys.Length == 0)
            {
                return;
            }

            await database.KeyDeleteAsync([.. redisKeys]);
        }
    }
}
