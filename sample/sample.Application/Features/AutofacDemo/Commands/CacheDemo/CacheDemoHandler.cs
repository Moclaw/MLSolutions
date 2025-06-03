using Autofac;
using Autofac.Features.Indexed;
using MediatR;
using sample.Infrastructure.Services;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.AutofacDemo.Commands.CacheDemo;

public class CacheDemoHandler(IIndex<string, ICacheService> cacheServices) 
    : IRequestHandler<CacheDemoRequest, Response<CacheDemoResponse>>
{
    public async Task<Response<CacheDemoResponse>> Handle(CacheDemoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = request.CacheType.ToLower() switch
            {
                "memory" => "MemoryCache",
                "redis" => "RedisCache",
                _ => throw new ArgumentException($"Invalid cache type: {request.CacheType}")
            };

            if (!cacheServices.TryGetValue(cacheKey, out var cache))
            {
                return ResponseUtils.Error<CacheDemoResponse>(400, $"Cache service '{cacheKey}' not found");
            }

            switch (request.Operation.ToLower())
            {
                case "set":
                    if (string.IsNullOrEmpty(request.Key) || string.IsNullOrEmpty(request.Value))
                        return ResponseUtils.Error<CacheDemoResponse>(400, "Key and Value are required for set operation");
                    
                    await cache.SetAsync(request.Key, request.Value, TimeSpan.FromMinutes(5));
                    return ResponseUtils.Success(new CacheDemoResponse
                    {
                        Success = true,
                        Operation = "Set",
                        Key = request.Key,
                        CacheType = request.CacheType
                    }, "Cache value set successfully");

                case "get":
                    if (string.IsNullOrEmpty(request.Key))
                        return ResponseUtils.Error<CacheDemoResponse>(400, "Key is required for get operation");
                    
                    var value = await cache.GetAsync<string>(request.Key);
                    return ResponseUtils.Success(new CacheDemoResponse
                    {
                        Success = true,
                        Operation = "Get",
                        Key = request.Key,
                        Value = value,
                        CacheType = request.CacheType
                    }, "Cache value retrieved successfully");

                case "remove":
                    if (string.IsNullOrEmpty(request.Key))
                        return ResponseUtils.Error<CacheDemoResponse>(400, "Key is required for remove operation");
                    
                    await cache.RemoveAsync(request.Key);
                    return ResponseUtils.Success(new CacheDemoResponse
                    {
                        Success = true,
                        Operation = "Remove",
                        Key = request.Key,
                        CacheType = request.CacheType
                    }, "Cache value removed successfully");

                default:
                    return ResponseUtils.Error<CacheDemoResponse>(400, $"Invalid operation: {request.Operation}");
            }        }
        catch (Exception ex)
        {
            return ResponseUtils.Error<CacheDemoResponse>(500, $"Error in cache operation: {ex.Message}");
        }
    }
}
