# Moclawr.Services.Caching

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Services.Caching.svg)](https://www.nuget.org/packages/Moclawr.Services.Caching/)

## Overview

Moclawr.Services.Caching provides a flexible caching solution for .NET applications, with support for both Redis and in-memory caching strategies. It offers a consistent API across different caching providers, making it easy to implement efficient data caching in your applications.

## Features

- **Multiple Cache Providers**: 
  - Redis distributed cache support
  - In-memory cache for development/testing
- **Flexible API**: Common interface for all cache operations
- **Advanced Key Management**: 
  - Pattern-based cache key operations
  - Various search operators (StartsWith, EndsWith, Contains, Exact)
- **Type-Safe Operations**: Generic methods for type-safe cache interactions
- **Expiration Control**: Set custom expiration times for cached items
- **Bulk Operations**: Remove multiple cache entries by pattern
- **Seamless Integration**: Easy integration with ASP.NET Core dependency injection

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.Services.Caching
```

## Usage

### Configure Redis Caching

In your `Program.cs` or `Startup.cs`:

```csharp
using Services.Caching;

// Add Redis caching
builder.Services.AddRedisCache(builder.Configuration);
```

Configuration in appsettings.json:

```json
{
  "Redis": {
    "Connection": "localhost:6379",
    "InstanceName": "MyApp:",
    "Database": 0,
    "DefaultCacheTime": 60
  }
}
```

### Using the Cache Service

Inject and use the `IRedisServices` interface in your services:

```csharp
using Services.Caching.Redis;

public class ProductService
{
    private readonly IRedisServices _redisServices;
    
    public ProductService(IRedisServices redisServices)
    {
        _redisServices = redisServices;
    }
    
    public async Task<Product> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Try to get from cache first
        var cacheKey = $"product:{id}";
        var cachedProduct = await _redisServices.GetCacheValueAsync<Product>(cacheKey, cancellationToken);
        
        if (cachedProduct != null)
        {
            return cachedProduct;
        }
        
        // If not in cache, get from database
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        
        // Cache the result for 30 minutes
        if (product != null)
        {
            await _redisServices.SetCacheValueAsync(cacheKey, product, 30, cancellationToken);
        }
        
        return product;
    }
    
    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        // Update in database
        await _productRepository.UpdateAsync(product, cancellationToken);
        
        // Update in cache
        var cacheKey = $"product:{product.Id}";
        await _redisServices.SetCacheValueAsync(cacheKey, product, 30, cancellationToken);
    }
    
    public async Task InvalidateProductCacheAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{id}";
        await _redisServices.RemoveCacheValueAsync(cacheKey, cancellationToken);
    }
    
    public async Task InvalidateAllProductCacheAsync(CancellationToken cancellationToken = default)
    {
        // Remove all keys starting with "product:"
        await _redisServices.RemoveCacheRangeAsync(
            "product:", 
            Core.Constants.CacheKeySearchOperator.StartsWith, 
            cancellationToken
        );
    }
}
```

### Advanced Cache Operations

Remove cache entries by pattern:

```csharp
// Remove all cache entries for a specific user
await _redisServices.RemoveCacheRangeAsync(
    $"user:{userId}:",
    Core.Constants.CacheKeySearchOperator.StartsWith
);

// Remove all cache entries containing a specific term
await _redisServices.RemoveCacheRangeAsync(
    "product",
    Core.Constants.CacheKeySearchOperator.Contains
);

// Remove cache entries with exact match
await _redisServices.RemoveCacheRangeAsync(
    "global:settings",
    Core.Constants.CacheKeySearchOperator.Exact
);
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Provides configuration models and constants used by the caching service
- **Moclawr.Host**: Can be used together for building complete API solutions
- **Moclawr.EfCore**: Use with Entity Framework Core for caching query results

## Requirements

- .NET 9.0 or higher
- StackExchange.Redis 2.8.37 or higher
- Microsoft.Extensions.Caching.StackExchangeRedis 9.0.0 or higher

## License

This package is licensed under the MIT License.
