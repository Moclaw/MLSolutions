# Moclawr.Services.Caching

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Services.Caching.svg)](https://www.nuget.org/packages/Moclawr.Services.Caching/)

## Overview

Moclawr.Services.Caching provides a comprehensive caching solution for .NET applications with support for both Redis and in-memory caching. It offers intelligent cache invalidation, flexible key management, and seamless integration with the Moclawr ecosystem.

## Features

- **Multiple Cache Providers**: Support for Redis and in-memory caching
- **Intelligent Invalidation**: Flexible cache removal strategies with pattern matching
- **Key Management**: Built-in cache key generation and organization
- **Async Operations**: Full async/await support for all cache operations
- **Serialization**: Automatic JSON serialization/deserialization
- **Health Checks**: Built-in health monitoring for cache services
- **Configuration**: Simple configuration through dependency injection

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.Services.Caching
```

## Usage

### Registering Services

In your `Program.cs`:

```csharp
using Services.Caching;

// Register caching services
builder.Services.AddCachingServices(builder.Configuration);
```

### Configuration

Configure caching in `appsettings.json`:

```json
{
  "Caching": {
    "DefaultProvider": "Redis",
    "DefaultExpirationMinutes": 30,
    "Redis": {
      "ConnectionString": "localhost:6379",
      "Database": 0,
      "KeyPrefix": "MyApp:",
      "InstanceName": "MyApp"
    },
    "InMemory": {
      "SizeLimit": 1024,
      "CompactionPercentage": 0.1
    }
  }
}
```

### Using Redis Cache Service

```csharp
using Services.Caching.Redis;

public class ProductService
{
    private readonly IRedisServices _cacheService;
    private readonly IProductRepository _productRepository;

    public ProductService(IRedisServices cacheService, IProductRepository productRepository)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        var cacheKey = _cacheService.BuildCacheKey("products", productId.ToString());
        
        var product = await _cacheService.GetCacheValueAsync<Product>(cacheKey);
        if (product != null)
        {
            return product;
        }

        product = await _productRepository.GetByIdAsync(productId);
        if (product != null)
        {
            await _cacheService.SetCacheValueAsync(cacheKey, product, 60); // Cache for 60 minutes
        }

        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        var updatedProduct = await _productRepository.UpdateAsync(product);
        
        // Invalidate related cache entries
        var cacheKey = _cacheService.BuildCacheKey("products", product.Id.ToString());
        await _cacheService.RemoveCacheValueAsync(cacheKey);
        
        // Also remove category-based cache entries
        await _cacheService.RemoveCacheRangeAsync(
            $"products:category:{product.CategoryId}", 
            CacheKeySearchOperator.StartsWith
        );

        return updatedProduct;
    }
}
```

### Cache-Aside Pattern

```csharp
public class UserService
{
    private readonly IRedisServices _cacheService;
    private readonly IUserRepository _userRepository;

    public UserService(IRedisServices cacheService, IUserRepository userRepository)
    {
        _cacheService = cacheService;
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> GetActiveUsersAsync()
    {
        var cacheKey = "users:active";
        
        return await _cacheService.GetOrSetCacheValueAsync(
            cacheKey,
            async () =>
            {
                var users = await _userRepository.GetActiveUsersAsync();
                return users.Select(u => new UserDto 
                { 
                    Id = u.Id, 
                    Name = u.Name, 
                    Email = u.Email 
                }).ToList();
            },
            cacheTimeInMinutes: 30
        );
    }

    public async Task<(bool found, UserDto? user)> TryGetUserFromCacheAsync(int userId)
    {
        var cacheKey = _cacheService.BuildCacheKey("users", userId.ToString());
        var (found, userData) = await _cacheService.TryGetCacheValueAsync<UserDto>(cacheKey);
        
        return (found, userData);
    }
}
```

### Advanced Cache Management

```csharp
public class CacheManager
{
    private readonly IRedisServices _cacheService;

    public CacheManager(IRedisServices cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task InvalidateUserCacheAsync(int userId)
    {
        // Remove specific user cache
        var userKey = _cacheService.BuildCacheKey("users", userId.ToString());
        await _cacheService.RemoveCacheValueAsync(userKey);
        
        // Remove user's related data
        await _cacheService.RemoveCacheRangeAsync(
            $"users:{userId}:", 
            CacheKeySearchOperator.StartsWith
        );
    }

    public async Task InvalidateAllProductCacheAsync()
    {
        // Remove all product-related cache entries
        await _cacheService.RemoveCacheRangeAsync(
            "products", 
            CacheKeySearchOperator.StartsWith
        );
    }

    public async Task WarmupCacheAsync()
    {
        // Pre-populate cache with frequently accessed data
        var popularProducts = await GetPopularProductsFromDatabase();
        
        foreach (var product in popularProducts)
        {
            var cacheKey = _cacheService.BuildCacheKey("products", product.Id.ToString());
            await _cacheService.SetCacheValueAsync(cacheKey, product, 120); // 2 hours
        }
    }

    private async Task<List<Product>> GetPopularProductsFromDatabase()
    {
        // Implementation to get popular products
        return new List<Product>();
    }
}
```

### Integration with Repository Pattern

```csharp
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _baseRepository;
    private readonly IRedisServices _cacheService;

    public CachedProductRepository(IProductRepository baseRepository, IRedisServices cacheService)
    {
        _baseRepository = baseRepository;
        _cacheService = cacheService;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = _cacheService.BuildCacheKey("products", id.ToString());
        
        return await _cacheService.GetOrSetCacheValueAsync(
            cacheKey,
            () => _baseRepository.GetByIdAsync(id, cancellationToken),
            30 // 30 minutes cache
        );
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = _cacheService.BuildCacheKey("products", "paged", page.ToString(), pageSize.ToString(), searchTerm ?? "all");
        
        return await _cacheService.GetOrSetCacheValueAsync(
            cacheKey,
            () => _baseRepository.GetPagedAsync(page, pageSize, searchTerm, cancellationToken),
            15 // 15 minutes cache for paged results
        );
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var result = await _baseRepository.CreateAsync(product, cancellationToken);
        
        // Invalidate related cache entries
        await _cacheService.RemoveCacheRangeAsync("products:paged", CacheKeySearchOperator.StartsWith);
        
        return result;
    }
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Uses extension methods and utilities for enhanced functionality
- **Moclawr.Shared**: Integrates with response models and entity interfaces for cache key generation
- **Moclawr.Domain**: Caches domain entities and supports domain-driven cache strategies
- **Moclawr.EfCore** and **Moclawr.MongoDb**: Perfect for caching repository query results
- **Moclawr.MinimalAPI**: Enables response caching for API endpoints
- **Moclawr.Host**: Integrates with health checks and application monitoring
- **Moclawr.Services.External**: Cache external service responses to reduce API calls

## Health Checks

The package includes built-in health checks for monitoring cache service availability:

```csharp
// Automatic registration with AddCachingServices
builder.Services.AddCachingServices(builder.Configuration);

// Health checks are automatically registered
app.MapHealthChecks("/health");
```

## Requirements

- .NET 9.0 or higher
- StackExchange.Redis 2.8.16 or higher (for Redis caching)
- Microsoft.Extensions.Caching.Memory 9.0.5 or higher (for in-memory caching)
- Microsoft.Extensions.Configuration 9.0.5 or higher

## License

This package is licensed under the MIT License.
