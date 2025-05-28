# Moclawr.Shared

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Shared.svg)](https://www.nuget.org/packages/Moclawr.Shared/)

## Overview

Moclawr.Shared provides fundamental interfaces, response models, and utility classes shared across all Moclawr libraries. It establishes consistent patterns for entity design, error handling, and API responses.

## Features

- **Entity Interfaces**: Base interfaces for entity modeling with typed IDs
- **Standard Response Models**: Unified response types for APIs and services
- **Exception Types**: Custom exceptions for common business scenarios
- **Utility Classes**: Helpers for paging, password security, and response formatting
- **JSON Settings**: Default serialization configuration for consistent data handling

## Installation

```shell
dotnet add package Moclawr.Shared
```

## Usage Examples

### Entity Interfaces

```csharp
// Create an entity that implements the IEntity interface
public class Customer : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

### Response Models

```csharp
// Create a success response with data
var response = new Response<CustomerDto>
{
    Data = customerDto,
    IsSuccess = true,
    StatusCode = 200,
    Message = "Customer retrieved successfully"
};

// Create an error response
var errorResponse = Response.Error("Customer not found", 404);
```

### Utility Classes

```csharp
// Create a paging object for list queries
var paging = new Paging(pageIndex: 1, pageSize: 20);

// Hash a password securely
string hashedPassword = PasswordHasher.HashPassword("userPassword");
bool isValid = PasswordHasher.VerifyPassword(hashedPassword, "userPassword");
```

## Integration with Other Moclawr Packages

Moclawr.Shared serves as a dependency for virtually all other Moclawr packages:

- **Moclawr.Core**: Provides extension methods and utilities that work with Shared interfaces
- **Moclawr.Domain**: Uses Shared entity interfaces for domain modeling and base classes
- **Moclawr.EfCore** and **Moclawr.MongoDb**: Implement Shared repository contracts and entity requirements
- **Moclawr.MinimalAPI**: Uses Shared response models for standardized API responses
- **Moclawr.Host**: Leverages Shared exception types and response models for error handling
- **Moclawr.Services.Caching**: Uses Shared response models for cached data structures
- **Moclawr.Services.External**: Implements Shared response patterns for external service calls

## Requirements

- .NET 9.0 or higher

## License

This project is licensed under the MIT License - see the LICENSE file for details.
