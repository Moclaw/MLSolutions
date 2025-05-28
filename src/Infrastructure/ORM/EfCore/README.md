# Moclawr.EfCore

[![NuGet](https://img.shields.io/nuget/v/Moclawr.EfCore.svg)](https://www.nuget.org/packages/Moclawr.EfCore/)

## Overview

Moclawr.EfCore provides a comprehensive set of tools and abstractions for working with Entity Framework Core in .NET applications. It implements the repository pattern with separate command and query repositories (CQRS), fluent query builders, and advanced querying capabilities, making database operations more maintainable and efficient.

## Features

- **Repository Pattern Implementation**: Separate repositories for commands and queries
- **CQRS Approach**: Command and Query Responsibility Segregation with specialized repositories
- **Fluent Query Building**: Expressive, chainable API for building complex queries
- **Advanced Filtering**: Specifications pattern for encapsulating filtering logic
- **Projection Support**: Map entities to DTOs directly in queries for optimal performance
- **Transaction Management**: Simplified transaction handling across multiple operations
- **Pagination**: Built-in support for paginated results
- **Dapper Integration**: Combine the power of EF Core and Dapper for complex queries
- **Auditing Support**: Automatic tracking of create/update timestamps and users
- **Soft Delete**: Built-in support for soft delete operations

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.EfCore
```

## Usage

### Setting Up the DbContext

Create a DbContext that inherits from BaseDbContext:

```csharp
using EfCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entities
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
    }
}
```

### Registering Services

In your `Program.cs` or `Startup.cs`:

```csharp
using EfCore;
using Microsoft.EntityFrameworkCore;

// Register EF Core services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<ICommandRepository<Product, int>, CommandRepository<Product, int>>();
builder.Services.AddScoped<IQueryRepository<Product, int>, QueryRepository<Product, int>>();
```

### Using the Query Repository

```csharp
using EfCore.Repositories;
using Shared.Utils;

public class ProductService
{
    private readonly IQueryRepository<Product, int> _queryRepository;
    
    public ProductService(IQueryRepository<Product, int> queryRepository)
    {
        _queryRepository = queryRepository;
    }
    
    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _queryRepository.GetByIdAsync(
            id,
            builder: query => query
                .Include(p => p.Category)
                .Include(p => p.Tags),
            cancellationToken: cancellationToken
        );
    }
    
    public async Task<PagedResult<ProductDto>> GetPaginatedProductsAsync(
        string? searchTerm, 
        int page, 
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _queryRepository.GetPagedListAsync<ProductDto>(
            page,
            pageSize,
            filter: p => string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm),
            orderBy: q => q.OrderByDescending(p => p.CreatedAt),
            builder: query => query.Include(p => p.Category),
            cancellationToken: cancellationToken
        );
    }
    
    public async Task<List<ProductSummaryDto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _queryRepository.GetProjectedListAsync<ProductSummaryDto>(
            filter: p => p.IsFeatured && p.IsActive,
            orderBy: q => q.OrderBy(p => p.DisplayOrder),
            cancellationToken: cancellationToken
        );
    }
}
```

### Using the Command Repository

```csharp
using EfCore.Repositories;

public class ProductManagementService
{
    private readonly ICommandRepository<Product, int> _commandRepository;
    
    public ProductManagementService(ICommandRepository<Product, int> commandRepository)
    {
        _commandRepository = commandRepository;
    }
    
    public async Task<int> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _commandRepository.AddAsync(product, cancellationToken);
        await _commandRepository.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
    
    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        _commandRepository.Update(product);
        await _commandRepository.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        await _commandRepository.DeleteAsync(id, cancellationToken);
        await _commandRepository.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<bool> ExecuteInTransactionAsync(Func<Task<bool>> operation, CancellationToken cancellationToken = default)
    {
        return await _commandRepository.ExecuteInTransactionAsync(operation, cancellationToken);
    }
}
```

### Using Fluent Query Builders

```csharp
using EfCore.Builders;

public async Task<List<Product>> GetProductsByComplexCriteriaAsync(
    string? categoryName, 
    decimal? minPrice,
    decimal? maxPrice,
    bool inStock,
    CancellationToken cancellationToken = default)
{
    return await _queryRepository.GetListAsync(
        builder: query => query
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .WhereIf(
                !string.IsNullOrEmpty(categoryName),
                p => p.Category.Name == categoryName
            )
            .WhereIf(
                minPrice.HasValue,
                p => p.Price >= minPrice.Value
            )
            .WhereIf(
                maxPrice.HasValue,
                p => p.Price <= maxPrice.Value
            )
            .WhereIf(
                inStock,
                p => p.Inventory.StockQuantity > 0
            ),
        cancellationToken: cancellationToken
    );
}
```

### Using Dapper for Complex Queries

```csharp
public async Task<List<ProductSalesReport>> GetProductSalesReportAsync(
    DateTime startDate,
    DateTime endDate,
    CancellationToken cancellationToken = default)
{
    var sql = @"
        SELECT p.Id, p.Name, p.Price, 
               COUNT(o.Id) AS OrderCount, 
               SUM(oi.Quantity) AS TotalQuantity,
               SUM(oi.Quantity * p.Price) AS TotalRevenue
        FROM Products p
        JOIN OrderItems oi ON p.Id = oi.ProductId
        JOIN Orders o ON oi.OrderId = o.Id
        WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
        GROUP BY p.Id, p.Name, p.Price
        ORDER BY TotalRevenue DESC";
    
    var parameters = new { StartDate = startDate, EndDate = endDate };
    
    return await _queryRepository.QueryAsync<ProductSalesReport>(
        sql, 
        parameters, 
        CommandType.Text,
        cancellationToken
    );
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Provides essential utilities and extension methods
- **Moclawr.Shared**: Uses entity interfaces, response models, and pagination utilities
- **Moclawr.Domain**: Contains base entity types and builder interfaces for domain modeling
- **Moclawr.Host**: Integrates with health checks and global exception handling
- **Moclawr.Services.Caching**: Perfect companion for caching query results and repositories
- **Moclawr.MinimalAPI**: Works with CQRS handlers and endpoint patterns for clean API design

## Requirements

- .NET 9.0 or higher
- Microsoft.EntityFrameworkCore 9.0.5 or higher
- Microsoft.EntityFrameworkCore.Relational 9.0.5 or higher
- Dapper 2.1.66 or higher
- Mapster 7.4.0 or higher

## License

This package is licensed under the MIT License.
