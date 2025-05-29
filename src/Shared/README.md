# Moclawr.Shared

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Shared.svg)](https://www.nuget.org/packages/Moclawr.Shared/)

## Overview

Moclawr.Shared provides fundamental building blocks and common abstractions used across the entire MLSolutions framework. It includes base entity interfaces, standardized response models, exception types, and utility classes that ensure consistency and interoperability between all other packages in the ecosystem.

## Features

- **Base Entity Interfaces**: Common contracts for entity modeling with typed IDs
- **Standardized Response Models**: Consistent API response structures for success, error, and collection results
- **Custom Exception Types**: Business-specific exceptions for common scenarios
- **Response Utilities**: Helper methods for creating standardized responses
- **Pagination Support**: Built-in pagination models and metadata
- **Password Security**: Secure password hashing utilities with bcrypt
- **JSON Configuration**: Default serialization settings for consistent data handling
- **Object Manipulation**: Utilities for mapping, copying, and transforming objects

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.Shared
```

## Core Components

### Entity Interfaces

#### Base Entity Interface
```csharp
public interface IEntity<TId>
{
    TId Id { get; set; }
}

// Usage in domain models
public class User : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class Product : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

#### Track Entity Interface
```csharp
public interface ITrackEntity<TId> : IEntity<TId>
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}

// Usage for auditable entities
public class AuditableProduct : ITrackEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

### Response Models

#### Standard Response Structure
```csharp
// Single item response
public record Response<T> : IResponse
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
}

// Collection response with pagination
public record ResponseCollection<T> : IResponse
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public IEnumerable<T> Data { get; init; } = [];
    public PaginationMetadata? Pagination { get; init; }
    public string? Error { get; init; }
}

// Simple response without data
public record Response : IResponse
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public string? Error { get; init; }
}
```

#### Creating Responses
```csharp
using Shared.Utils;

// Success responses
var userResponse = ResponseHelper.CreateSuccess(user, "User retrieved successfully");
var usersResponse = ResponseHelper.CreateSuccessCollection(users, pagination);
var operationResponse = ResponseHelper.CreateSuccess("Operation completed");

// Error responses
var notFoundResponse = ResponseHelper.CreateNotFound<User>("User not found");
var validationResponse = ResponseHelper.CreateValidationError("Invalid email format");
var serverErrorResponse = ResponseHelper.CreateServerError<User>("Database connection failed");

// Custom responses
var customResponse = ResponseHelper.CreateResponse<User>(
    data: user,
    isSuccess: true,
    message: "Custom success message",
    statusCode: 200
);
```

### Exception Types

#### Business Logic Exceptions
```csharp
// Entity not found
public class EntityNotFoundException(string entityName, object id) 
    : Exception($"{entityName} with ID '{id}' was not found");

// Usage
throw new EntityNotFoundException(nameof(User), userId);

// Entity conflict (duplicate)
public class EntityConflictException(string entityName, string field, object value)
    : Exception($"{entityName} with {field} '{value}' already exists");

// Usage
throw new EntityConflictException(nameof(User), "Email", email);

// Business rule violation
public class BusinessRuleException(string rule, string? details = null)
    : Exception($"Business rule violation: {rule}" + 
                (details != null ? $". {details}" : string.Empty));

// Usage
throw new BusinessRuleException(
    "User cannot delete their own account", 
    "Please contact an administrator"
);
```

#### Validation Exceptions
```csharp
public class ValidationException(IDictionary<string, string[]> errors) 
    : Exception("Validation failed")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}

// Usage
var errors = new Dictionary<string, string[]>
{
    ["Email"] = ["Email is required", "Email format is invalid"],
    ["Password"] = ["Password must be at least 8 characters"]
};
throw new ValidationException(errors);
```

### Pagination Support

#### Pagination Models
```csharp
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
) {
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public record PaginationMetadata(
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPrevious,
    bool HasNext,
    string? PreviousPageUrl = null,
    string? NextPageUrl = null
);
```

#### Creating Paginated Results
```csharp
// From query results
var pagedUsers = new PagedResult<UserDto>(
    items: users,
    totalCount: totalUserCount,
    page: 1,
    pageSize: 10
);

// With URL generation
var metadata = new PaginationMetadata(
    CurrentPage: 1,
    PageSize: 10,
    TotalCount: 150,
    TotalPages: 15,
    HasPrevious: false,
    HasNext: true,
    PreviousPageUrl: null,
    NextPageUrl: "/api/users?page=2&pageSize=10"
);
```

### Password Security

#### Secure Password Hashing
```csharp
using Shared.Utils;

// Hash a password
string plainPassword = "mySecurePassword123";
string hashedPassword = PasswordHelper.HashPassword(plainPassword);

// Verify a password
bool isValid = PasswordHelper.VerifyPassword(plainPassword, hashedPassword);

// Example in user registration
public class UserService
{
    public async Task<User> CreateUserAsync(string email, string password)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };
        
        // Save user...
        return user;
    }
    
    public async Task<bool> ValidateLoginAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return false;
        
        return PasswordHelper.VerifyPassword(password, user.PasswordHash);
    }
}
```

### Object Utilities

#### Object Manipulation
```csharp
using Shared.Utils;

// Copy object properties
var sourceUser = new UserDto { Name = "John", Email = "john@example.com" };
var targetUser = new User();
ObjectHelper.CopyProperties(sourceUser, targetUser);

// Map object to dictionary
var userDict = ObjectHelper.ToDictionary(sourceUser);

// Create object from dictionary
var recreatedUser = ObjectHelper.FromDictionary<UserDto>(userDict);

// Deep clone object
var clonedUser = ObjectHelper.DeepClone(sourceUser);
```

### JSON Configuration

#### Default Settings
```csharp
using Shared.Settings;

// Pre-configured JSON options
var jsonOptions = DefaultJsonSettings.JsonSerializerOptions;

// Features included:
// - camelCase property naming
// - Ignore null values
// - Case-insensitive property matching
// - Pretty formatting (in development)
// - Custom date/time formatting
// - Enum string conversion

// Usage in API configuration
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions = DefaultJsonSettings.JsonSerializerOptions;
});
```

## Usage Patterns

### API Response Standardization
```csharp
[HttpGet("{id}")]
public async Task<Response<UserDto>> GetUser(int id)
{
    try
    {
        var user = await _userService.GetByIdAsync(id);
        return ResponseHelper.CreateSuccess(user, "User retrieved successfully");
    }
    catch (EntityNotFoundException)
    {
        return ResponseHelper.CreateNotFound<UserDto>("User not found");
    }
    catch (Exception ex)
    {
        return ResponseHelper.CreateServerError<UserDto>("An error occurred while retrieving the user");
    }
}

[HttpGet]
public async Task<ResponseCollection<UserDto>> GetUsers(
    int page = 1, 
    int pageSize = 10)
{
    try
    {
        var pagedUsers = await _userService.GetPagedAsync(page, pageSize);
        
        var metadata = new PaginationMetadata(
            page, pageSize, pagedUsers.TotalCount, pagedUsers.TotalPages,
            pagedUsers.HasPreviousPage, pagedUsers.HasNextPage
        );
        
        return ResponseHelper.CreateSuccessCollection(pagedUsers.Items, metadata);
    }
    catch (Exception ex)
    {
        return ResponseHelper.CreateServerErrorCollection<UserDto>("Failed to retrieve users");
    }
}
```

### Exception Handling in Services
```csharp
public class UserService
{
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Check for existing user
        var existingUser = await _repository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new EntityConflictException(nameof(User), "Email", request.Email);
        }
        
        // Validate business rules
        if (request.Age < 18)
        {
            throw new BusinessRuleException(
                "User must be at least 18 years old",
                "Age verification is required for registration"
            );
        }
        
        // Create user
        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };
        
        return await _repository.AddAsync(user);
    }
}
```

## Integration with Other Moclawr Packages

This package is foundational and used by all other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Extends utilities and provides additional extension methods
- **Moclawr.Domain**: Uses entity interfaces and implements domain-specific extensions
- **Moclawr.EfCore** and **Moclawr.MongoDb**: Implement repository patterns using shared interfaces
- **Moclawr.Host**: Uses exception types for global exception handling
- **Moclawr.MinimalAPI**: Uses response models and creates standardized API responses
- **Moclawr.Services.Caching**: Uses entity interfaces for cache key generation
- **Moclawr.Services.External**: Uses response models for service communication

## Requirements

- .NET 9.0 or higher
- BCrypt.Net-Next 4.0.3 or higher (for password hashing)

## License

This package is licensed under the MIT License.
