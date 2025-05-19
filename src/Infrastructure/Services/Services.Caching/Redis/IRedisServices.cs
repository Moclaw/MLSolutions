using Core.Constants;

namespace Services.Caching.Redis
{
    /// <summary>
    /// Provides methods for interacting with a Redis cache, including getting, setting, and removing cache values.
    /// </summary>
    public interface IRedisServices
    {
        /// <summary>
        /// Asynchronously retrieves a cached value by key.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>The cached value if found; otherwise, <c>null</c>.</returns>
        Task<T?> GetCacheValueAsync<T>(string key, CancellationToken cancellation = default);

        /// <summary>
        /// Asynchronously sets a value in the cache with a specified expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="data">The value to cache.</param>
        /// <param name="cacheTimeInMinutes">The cache expiration time in minutes.</param>
        /// <param name="cancellation">A cancellation token.</param>
        Task SetCacheValueAsync<T>(
            string key,
            T data,
            int cacheTimeInMinutes,
            CancellationToken cancellation = default
        );

        /// <summary>
        /// Asynchronously removes a value from the cache by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellation">A cancellation token.</param>
        Task RemoveCacheValueAsync(string key, CancellationToken cancellation = default);

        /// <summary>
        /// Removes a range of cache entries matching a pattern and search operator.
        /// Note: Using the Contain operator may cause performance issues with Redis.
        /// </summary>
        /// <param name="keyPattern">The keyword to search for in cache keys. Wildcards are not required.</param>
        /// <param name="searchOperator">The search operator to use for matching keys.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveCacheRangeAsync(
            string keyPattern,
            CacheKeySearchOperator searchOperator = CacheKeySearchOperator.StartsWith,
            CancellationToken cancellation = default
        );

        /// <summary>
        /// Attempts to retrieve a cached value by key, indicating whether the value was found.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>
        /// A tuple containing a boolean indicating if the value was found and the cached value if present.
        /// </returns>
        Task<(bool found, T? cacheData)> TryGetCacheValueAsync<T>(
            string key,
            CancellationToken cancellation = default
        );

        /// <summary>
        /// Gets a cached value if it exists; otherwise, sets the value using the provided factory and caches it.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="valueFactory">A factory function to generate the value if not cached.</param>
        /// <param name="cacheTimeInMinutes">The cache expiration time in minutes.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>The cached or newly generated value.</returns>
        Task<T?> GetOrSetCacheValueAsync<T>(
            string key,
            Func<Task<T>> valueFactory,
            int cacheTimeInMinutes,
            CancellationToken cancellation = default
        );

        /// <summary>
        /// Builds a cache key from the provided key segments.
        /// </summary>
        /// <param name="keys">The segments to include in the cache key.</param>
        /// <returns>The constructed cache key.</returns>
        string BuildCacheKey(params string[] keys);
    }
}
