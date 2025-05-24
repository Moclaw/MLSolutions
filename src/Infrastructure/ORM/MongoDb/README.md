# Moclawr.MongoDb

[![NuGet](https://img.shields.io/nuget/v/Moclawr.MongoDb.svg)](https://www.nuget.org/packages/Moclawr.MongoDb/)

## Overview

Moclawr.MongoDb provides a comprehensive set of tools and abstractions for working with MongoDB in .NET applications, using the MongoDB.Driver with an Entity Framework Core-like approach. It implements the repository pattern with separate command and query repositories, making database operations more maintainable and consistent with the EF Core implementation in the Moclawr ecosystem.

## Features

- **Repository Pattern Implementation**: Separate repositories for commands and queries
- **CQRS Approach**: Command and Query Responsibility Segregation with specialized repositories
- **MongoDB Driver Integration**: Leverage the power of the official MongoDB driver
- **Entity Framework Core Compatibility**: Use MongoDB with a familiar EF Core-like API
- **Projection Support**: Map entities to DTOs directly in queries for optimal performance
- **Transaction Support**: MongoDB transaction handling for multi-document operations
- **Pagination**: Built-in support for paginated results
- **Advanced Filtering**: Expressive query API for building complex filters
- **Auditing Support**: Automatic tracking of create/update timestamps and users

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.MongoDb
```

## Usage

### Setting Up the MongoDB Context

Create a class that inherits from MongoBaseContext:

```csharp
using Microsoft.EntityFrameworkCore;
using MongoDb;
using MongoDB.Driver;

public class ProductsDbContext : MongoBaseContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }
    
    public IMongoCollection<Product> Products => MongoDatabase!.GetCollection<Product>("Products");
    public IMongoCollection<Category> Categories => MongoDatabase!.GetCollection<Category>("Categories");
}
```

### Registering Services

In your `Program.cs` or `Startup.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDb;
using Domain.IRepositories;
using MongoDb.Repositories;

// Register MongoDB services
builder.Services.AddDbContext<ProductsDbContext>(options =>
{
    var mongoClient = new MongoClient(builder.Configuration.GetConnectionString("MongoDb"));
    options.UseMongo(mongoClient, "ProductsDatabase");
});

// Register repositories
builder.Services.AddScoped<ICommandMongoRepository<Product, string>, CommandMongoRepository<Product, string>>();
builder.Services.AddScoped<IQueryMongoRepository<Product, string>, QueryMongoRepository<Product, string>>();
```

### Using the Query Repository

```csharp
using Domain.IRepositories;
using System.Linq.Expressions;

public class ProductService
{
    private readonly IQueryMongoRepository<Product, string> _queryRepository;
    
    public ProductService(IQueryMongoRepository<Product, string> queryRepository)
    {
        _queryRepository = queryRepository;
    }
    
    public async Task<Product?> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _queryRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
    }
    
    public async Task<List<Product>> GetProductsByCategoryAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        Expression<Func<Product, bool>> filter = p => p.CategoryId == categoryId;
        var result = await _queryRepository.GetAllAsync(filter, cancellationToken: cancellationToken);
        return result.ToList();
    }
    
    public async Task<List<ProductDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _queryRepository.GetProjectedListAsync<ProductDto>(
            filter: p => p.IsActive,
            orderBy: q => q.OrderBy(p => p.Name),
            cancellationToken: cancellationToken
        );
    }
    
    public async Task<ProductDetailsDto?> GetProductDetailsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _queryRepository.GetByIdAsync<ProductDetailsDto>(
            id,
            cancellationToken: cancellationToken
        );
    }
}
```

### Using the Command Repository

```csharp
using Domain.IRepositories;

public class ProductManagementService
{
    private readonly ICommandMongoRepository<Product, string> _commandRepository;
    
    public ProductManagementService(ICommandMongoRepository<Product, string> commandRepository)
    {
        _commandRepository = commandRepository;
    }
    
    public async Task<string> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _commandRepository.AddAsync(product, cancellationToken);
        await _commandRepository.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
    
    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _commandRepository.UpdateAsync(product, cancellationToken);
        await _commandRepository.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteProductAsync(string id, CancellationToken cancellationToken = default)
    {
        await _commandRepository.DeleteAsync(id, cancellationToken);
        await _commandRepository.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpdateInventoryAsync(string productId, int quantity, CancellationToken cancellationToken = default)
    {
        await _commandRepository.ExecuteTransactionAsync(async () => 
        {
            var product = await _commandRepository.GetByIdAsync(productId, true, cancellationToken);
            if (product != null)
            {
                product.StockQuantity = quantity;
                await _commandRepository.UpdateAsync(product, cancellationToken);
                await _commandRepository.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }, cancellationToken);
    }
}
```

### Advanced Queries

```csharp
public async Task<List<Product>> GetProductsByComplexCriteriaAsync(
    string? categoryId, 
    decimal? minPrice,
    decimal? maxPrice,
    bool inStock,
    CancellationToken cancellationToken = default)
{
    Expression<Func<Product, bool>> filter = p => 
        (string.IsNullOrEmpty(categoryId) || p.CategoryId == categoryId) &&
        (!minPrice.HasValue || p.Price >= minPrice.Value) &&
        (!maxPrice.HasValue || p.Price <= maxPrice.Value) &&
        (!inStock || p.StockQuantity > 0);
    
    var result = await _queryRepository.GetAllAsync(
        filter,
        cancellationToken: cancellationToken
    );
    
    return result.ToList();
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Domain**: Contains base entity types and repository interfaces
- **Moclawr.Shared**: Contains interfaces for entities and pagination utilities
- **Moclawr.EfCore**: Can be used alongside EF Core for hybrid database scenarios

## Requirements

- .NET 9.0 or higher
- MongoDB.Driver 3.3.0 or higher
- MongoDB.EntityFrameworkCore 9.0.0 or higher
- Microsoft.EntityFrameworkCore 9.0.4 or higher

## License

This package is licensed under the MIT License.
