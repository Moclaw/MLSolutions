using Services.Constants;

namespace Services.Redis;

public interface IRedisCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default);

    Task SetAsync<T>(
        string key,
        T data,
        int cacheTimeInMinutes,
        CancellationToken cancellation = default
    );

    Task RemoveAsync(string key, CancellationToken cancellation = default);

    /// <summary>
    /// Note: Cause perfomance issue, you should not use Contain operator if your cache provider is reids.
    /// </summary>
    /// <param name="keyPattern">Only input keyword. You do not need to add wilcard into keyword</param>
    /// <param name="searchOperator"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task RemoveRangeAsync(
        string keyPattern,
        CacheKeySearchOperator searchOperator = CacheKeySearchOperator.StartsWith,
        CancellationToken cancellation = default
    );

    Task<(bool, T? cacheData)> TryGetValueAsync<T>(
        string key,
        CancellationToken cancellation = default
    );

    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> func,
        int cacheTimeInMinutes,
        CancellationToken cancellation = default
    );

    string GenerateKey(params string[] keys);
}
