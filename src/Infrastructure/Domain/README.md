# Moclawr.Domain

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Domain.svg)](https://www.nuget.org/packages/Moclawr.Domain/)

## Overview

Moclawr.Domain provides essential domain modeling components for building robust business logic layers in .NET applications. It includes base entity classes, builder interfaces, and domain event infrastructure to support Domain-Driven Design (DDD) principles.

## Features

- **Base Entity Types**: Ready-to-use base classes for domain entities
- **Tracking Entities**: Support for audit fields (created/modified timestamps and users)
- **Builder Interfaces**: Fluent interfaces for constructing entities and queries
- **Domain Events**: Infrastructure for implementing domain events
- **Specifications**: Pattern implementation for complex business rules

## Installation

```shell
dotnet add package Moclawr.Domain
```

## Usage Examples

### Base Entities

```csharp
// Create a domain entity with audit tracking
public class Customer : TrackEntity<Guid>
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    
    public Customer(string name, string email)
    {
        Name = name;
        Email = email;
    }
    
    public void UpdateContact(string email)
    {
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Builder Interfaces

```csharp
// Implement a fluent builder for an entity
public class CustomerBuilder : IEntityBuilder<Customer>
{
    private string _name = string.Empty;
    private string _email = string.Empty;

    public CustomerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public Customer Build()
    {
        return new Customer(_name, _email);
    }
}
```

### Query Building

```csharp
// Use the fluent query builder
var customers = await repository.GetAllAsync(builder => 
    builder
        .Where(c => c.IsActive)
        .OrderBy(c => c.Name)
        .Include(c => c.Orders)
);
```

## Integration with Other Moclawr Packages

Moclawr.Domain is designed to work with:

- **Moclawr.Core**: Leverages extension methods and utilities for enhanced functionality
- **Moclawr.Shared**: Builds on base entity interfaces and standardized response models
- **Moclawr.EfCore** and **Moclawr.MongoDb**: Provides base classes for data models and repository patterns
- **Moclawr.MinimalAPI**: Perfect for CQRS handlers and domain-driven endpoint design
- **Moclawr.Host**: Integrates with domain event handling and business logic validation

## Requirements

- .NET 9.0 or higher

## License

This project is licensed under the MIT License - see the LICENSE file for details.
