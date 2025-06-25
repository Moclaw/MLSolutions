# Moclawr.MinimalAPI

[![NuGet](https://img.shields.io/nuget/v/Moclawr.MinimalAPI.svg)](https://www.nuget.org/packages/Moclawr.MinimalAPI/)

## Overview

Moclawr.MinimalAPI provides a class-based approach to ASP.NET Core Minimal APIs with automatic endpoint discovery, MediatR integration, and enhanced OpenAPI documentation. It bridges the gap between minimal APIs and traditional controller-based APIs by offering a structured, object-oriented approach while maintaining the performance benefits of minimal APIs.

## Key Features

- **Class-Based Endpoints**: Object-oriented endpoint design with base classes (`SingleEndpointBase`, `CollectionEndpointBase`)
- **Automatic Discovery**: Auto-registration of endpoint classes from assemblies via reflection
- **MediatR Integration**: Built-in CQRS pattern support with `ICommand<T>` and `IQueryRequest<T>` interfaces
- **Advanced API Versioning**: Multiple versioning strategies (URL segment, query string, headers)
- **Enhanced OpenAPI Documentation**: Rich SwaggerUI with automatic schema generation and versioned documents
- **Dynamic Feature Tagging**: Completely dynamic endpoint grouping based on actual namespace structure
- **Unique Operation IDs**: Deterministic hash-based operation IDs prevent duplicate endpoint names
- **Intelligent Request Binding**: Automatic parameter binding from route, query, body, form, and headers with attributes
- **Type-Safe Endpoints**: Strongly-typed request and response handling with generic base classes
- **Flexible Response Types**: Support for `Response<T>`, `ResponseCollection<T>`, and custom response wrappers
- **Attribute-Based Configuration**: Declarative endpoint configuration with comprehensive OpenAPI attributes
- **File Upload Support**: Built-in support for `IFormFile` and multi-file uploads

## Latest Updates (v2.1.2)

- ✅ **Complete Dynamic Discovery**: No hardcoded feature names - adapts to any namespace structure
- ✅ **Enhanced Route Matching**: Improved endpoint-to-operation matching with normalized route patterns
- ✅ **Deterministic Operation IDs**: Hash-based unique identifiers with readable prefixes
- ✅ **Advanced Parameter Binding**: Smart binding with `FromRoute`, `FromQuery`, `FromBody`, `FromForm`, `FromHeader` attributes
- ✅ **Comprehensive OpenAPI Support**: Full attribute system for documentation and parameter specification

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.MinimalAPI
```

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
using MinimalAPI;

var builder = WebApplication.CreateBuilder(args);

// Configure enhanced versioning options
var versioningOptions = new DefaultVersioningOptions
{
    Prefix = "v",
    DefaultVersion = 1,
    SupportedVersions = [1, 2],
    IncludeVersionInRoute = true,
    BaseRouteTemplate = "/api",
    ReadingStrategy = VersionReadingStrategy.UrlSegment | VersionReadingStrategy.QueryString,
    AssumeDefaultVersionWhenUnspecified = true
};

// Add MinimalAPI with comprehensive documentation
builder.Services.AddMinimalApiWithSwaggerUI(
    title: "My API",
    version: "v1",
    description: "API built with MinimalAPI framework",
    contactName: "Development Team",
    contactEmail: "dev@company.com",
    versioningOptions: versioningOptions,
    assemblies: [typeof(Program).Assembly]
);

var app = builder.Build();

// Auto-discover and map all endpoints with versioning
app.MapMinimalEndpoints(versioningOptions, typeof(Program).Assembly);

// Enable comprehensive documentation
if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiDocs(
        swaggerRoutePrefix: "docs",
        enableTryItOut: true,
        enableDeepLinking: true,
        enableFilter: true
    );
}

app.Run();
```

### 2. Create Endpoints with Full Attribute Support

```csharp
using MinimalAPI.Endpoints;
using MinimalAPI.Attributes;
using MediatR;

namespace MyApp.Endpoints.Users.Commands;

[OpenApiSummary("Create user account", 
    Description = "Creates a new user with validation and notification",
    Tags = ["User Management"])]
[OpenApiParameter("sendEmail", typeof(bool), 
    Description = "Send welcome email", Location = ParameterLocation.Query)]
[OpenApiResponse(201, ResponseType = typeof(Response<UserDto>), 
    Description = "User created successfully")]
[OpenApiResponse(400, Description = "Invalid user data")]
[ApiVersion(1)]
public class CreateUserEndpoint(IMediator mediator) 
    : SingleEndpointBase<CreateUserCommand, UserDto>(mediator)
{
    [HttpPost("users")]
    public override async Task<Response<UserDto>> HandleAsync(
        CreateUserCommand request, CancellationToken ct)
    {
        return await _mediator.Send(request, ct);
    }
}
```

### 3. Advanced Request Binding with Priority Rules

The framework now follows a clear priority system for parameter binding:

**Priority Order:**
1. **Explicit Attributes** (highest priority): `[FromRoute]`, `[FromQuery]`, `[FromHeader]`, `[FromForm]`, `[FromBody]`
2. **Type-based Detection**: `IFormFile` types automatically use form binding
3. **HTTP Method Defaults**:
   - **GET**: Properties default to query parameters
   - **POST/PUT/PATCH**: Properties default to request body (JSON) unless form data is detected

```csharp
using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;

// Command example - follows POST/PUT/PATCH rules
public record CreateUserCommand : ICommand<UserDto>
{
    // These will be in request body (JSON) - default for commands
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    
    // This will be a query parameter - explicit attribute overrides default
    [FromQuery] public bool SendWelcomeEmail { get; init; } = true;
    
    // This will be from header - explicit attribute
    [FromHeader("X-Client-Version")] public string? ClientVersion { get; init; }
    
    // This will be from route parameter - explicit attribute
    [FromRoute("tenantId")] public string? TenantId { get; init; }
}

// File upload example - auto-detects form data
public record UploadFileCommand : ICommand<FileDto>
{
    // These will be form fields - explicit FromForm attributes
    [FromForm] public IFormFile File { get; init; } = default!;
    [FromForm] public string Description { get; init; } = string.Empty;
    
    // This will be a query parameter - explicit attribute
    [FromQuery] public string? Folder { get; init; }
}

// Mixed form and body example
public record UpdateProfileCommand : ICommand<UserDto>
{
    // Route parameter
    [FromRoute] public int UserId { get; init; }
    
    // Form file upload
    [FromForm] public IFormFile? Avatar { get; init; }
    
    // Form fields
    [FromForm] public string? Bio { get; init; }
    
    // JSON body properties (remaining properties without explicit attributes)
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    
    // Query parameter
    [FromQuery] public bool NotifyFollowers { get; init; } = true;
}

// Query example - follows GET rules
public record GetUsersQuery : IQueryCollectionRequest<UserDto>
{
    // These will be query parameters automatically - default for queries
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    
    // This will be from route - explicit attribute overrides default
    [FromRoute] public string TenantId { get; init; } = string.Empty;
}
```

**Auto-Detection Rules:**
- `IFormFile`, `IFormFile[]`, `List<IFormFile>` → Automatically treated as form data
- Properties ending with "Id" or named "id" → Prefer route parameters when no explicit attribute
- Commands (POST/PUT/PATCH) → Body parameters by default
- Queries (GET) → Query parameters by default
- Form content type detected → All non-attributed properties become form fields

## Dynamic Feature Discovery

The framework automatically discovers and organizes endpoints based on your actual namespace structure:

### Completely Dynamic Tagging

No hardcoded feature names - the framework adapts to any organization:

```csharp
// Any namespace pattern works:
namespace MyApp.Endpoints.UserManagement.Operations    → "UserManagement Operations"
namespace CompanyApi.Endpoints.Finance.Reports        → "Finance Reports"  
namespace System.Endpoints.Authentication.Security    → "Authentication Security"
namespace Api.Endpoints.S3.Commands                   → "S3 Commands"
namespace Project.Endpoints.Analytics.Dashboards     → "Analytics Dashboards"
namespace sample.API.Endpoints.Todos.Commands         → "Todos Commands"
namespace sample.API.Endpoints.Tags.Queries           → "Tags Queries"
namespace sample.API.Endpoints.AutofacDemo.Commands   → "AutofacDemo Commands"
```

### Explicit Tag Override

Always use explicit tags when you want specific grouping:

```csharp
[OpenApiSummary("Complex operation", 
    Tags = ["Custom Category", "Special Operations"])]
public class ComplexEndpoint : SingleEndpointBase<ComplexCommand, ComplexResponse>
{
    // Explicit tags override namespace-based generation
}
```

## Unique Operation IDs

Deterministic hash-based operation IDs ensure no conflicts:

```csharp
Operation ID Format: {FeatureName}_{EndpointName}_{HttpMethod}_{Hash8}

Examples:
- Users_CreateUser_POST_a1b2c3d4
- S3_DeleteFile_DELETE_f9e8d7c6  
- Orders_UpdateStatus_PUT_3c7b8a9d
- Products_GetDetails_GET_b5f2e1a8
```

The 8-character hash is generated deterministically from:
- Full endpoint type name
- HTTP method
- Route template

This ensures consistent, unique identifiers across deployments.

## Advanced Versioning System

### Multiple Version Support

```csharp
[ApiVersion(1)]
public class GetUsersV1Endpoint : CollectionEndpointBase<GetUsersQuery, UserDtoV1>
{
    [HttpGet("users")]
    public override async Task<ResponseCollection<UserDtoV1>> HandleAsync(...)
    {
        // Version 1 implementation with basic fields
    }
}

[ApiVersion(2)]
public class GetUsersV2Endpoint : CollectionEndpointBase<GetUsersQueryV2, UserDtoV2>
{
    [HttpGet("users")]
    public override async Task<ResponseCollection<UserDtoV2>> HandleAsync(...)
    {
        // Version 2 with enhanced features and additional fields
    }
}
```

### Flexible Version Reading

The framework supports multiple versioning strategies:

```csharp
var versioningOptions = new DefaultVersioningOptions
{
    ReadingStrategy = VersionReadingStrategy.UrlSegment |   // /api/v1/users
                     VersionReadingStrategy.QueryString |  // /api/users?version=1  
                     VersionReadingStrategy.Header,        // X-API-Version: 1
    
    // Route generation
    BaseRouteTemplate = "/api",
    IncludeVersionInRoute = true,
    
    // Version validation
    SupportedVersions = [1, 2, 3],
    AssumeDefaultVersionWhenUnspecified = true
};
```

## Enhanced OpenAPI Documentation

### Comprehensive Attribute System

```csharp
[OpenApiSummary("Upload user profile image", 
    Description = "Uploads and processes a user profile image with validation")]
[OpenApiParameter("userId", typeof(int), 
    Description = "User identifier", Required = true, Location = ParameterLocation.Path)]
[OpenApiParameter("generateThumbnail", typeof(bool), 
    Description = "Generate thumbnail", Location = ParameterLocation.Query)]
[OpenApiResponse(200, ResponseType = typeof(Response<ImageDto>), 
    Description = "Image uploaded successfully")]
[OpenApiResponse(400, Description = "Invalid file format")]
[OpenApiResponse(413, Description = "File too large")]
[ApiVersion(1)]
public class UploadProfileImageEndpoint : SingleEndpointBase<UploadImageCommand, ImageDto>
{
    [HttpPost("users/{userId}/profile-image")]
    public override async Task<Response<ImageDto>> HandleAsync(...) => 
        await _mediator.Send(request, ct);
}
```

### Automatic Schema Generation

The framework automatically generates OpenAPI schemas for:

- **Request Types**: Command and query objects with proper binding
- **Response Types**: Response wrappers and DTOs
- **File Uploads**: IFormFile handling with multipart/form-data
- **Collections**: Paginated and filtered collection responses
- **Validation**: Required fields, nullable types, and constraints

## File Upload & Form Data

### Single and Multiple File Support

```csharp
public record UploadDocumentsCommand : ICommand<List<DocumentDto>>
{
    [FromForm] public List<IFormFile> Files { get; init; } = [];
    [FromForm] public string Category { get; init; } = string.Empty;
    [FromForm] public bool IsPublic { get; init; } = false;
    [FromQuery] public string? FolderId { get; init; }
}

// Generates proper multipart/form-data schema in OpenAPI
public class UploadDocumentsEndpoint : SingleEndpointBase<UploadDocumentsCommand, List<DocumentDto>>
{
    [HttpPost("documents/upload")]
    public override async Task<Response<List<DocumentDto>>> HandleAsync(...) =>
        await _mediator.Send(request, ct);
}
```

## Integration Examples

### With Autofac and Infrastructure Services

```csharp
// Program.cs - Complete setup
builder.UseAutofacServiceProvider(containerBuilder =>
{
    containerBuilder
        .AddApplicationServices()     // MediatR handlers
        .AddInfrastructureServices(); // Repositories, services
});

builder.Services
    .AddMinimalApiWithSwaggerUI(/* ... */)
    .AddInfrastructureServices(configuration) // Traditional DI
    .AddApplicationServices(configuration);
```

### Command Handler Example

```csharp
public class CreateUserHandler(
    ICommandRepository repository,
    ICacheService cache,
    ILogger<CreateUserHandler> logger)
    : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<Response<UserDto>> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cache.RemoveByPatternAsync("users:*");

        logger.LogInformation("Created user {UserId} with email {Email}", 
            user.Id, user.Email);

        return Response<UserDto>.Success(user.ToDto());
    }
}
```

## Architecture Components

### Base Classes

- `EndpointAbstractBase`: Core abstract base with execution framework
- `SingleEndpointBase<TRequest, TResponse>`: For single item responses  
- `CollectionEndpointBase<TRequest, TResponse>`: For collection responses
- `SingleEndpointBase<TRequest>`: For responses without specific data

### Interface Contracts

- `ICommand` / `ICommand<T>`: Command pattern interfaces
- `IQueryRequest<T>` / `IQueryCollectionRequest<T>`: Query interfaces
- `IEndpoint`: Core endpoint interface with HttpContext and Definition

### Attribute System

- `@HttpGet/@HttpPost/@HttpPut/@HttpDelete`: HTTP method routing
- `@OpenApiSummary`: Documentation and tagging
- `@OpenApiParameter/@OpenApiResponse`: Detailed API specification
- `@FromRoute/@FromQuery/@FromBody/@FromForm/@FromHeader`: Parameter binding
- `@ApiVersion`: Version specification

## Requirements & Dependencies

- **.NET 9.0** or higher
- **MediatR 12.2.0** - CQRS pattern implementation
- **Microsoft.AspNetCore.OpenApi 9.0.0** - OpenAPI document generation
- **Swashbuckle.AspNetCore 8.1.2** - SwaggerUI integration
- **System.Text.Json** - JSON serialization
- **Microsoft.Extensions.DependencyInjection** - Service registration

## Integration with Moclawr Ecosystem

Seamless integration with the complete Moclawr framework:

- **Moclawr.Core**: Extension methods and utilities
- **Moclawr.Shared**: `Response<T>` and `ResponseCollection<T>` models
- **Moclawr.Host**: Global exception handling and health checks
- **Moclawr.EfCore**: Repository patterns and Entity Framework integration
- **Moclawr.Services.Caching**: Response caching for API endpoints
- **Moclawr.Services.AWS.S3**: File storage integration
- **Moclawr.Services.AWS.SecretsManager**: Secure configuration and secret management
- **Moclawr.Services.Autofac**: Enhanced dependency injection and service discovery

## License

This package is licensed under the MIT License.

---

**Note**: Version 2.1.2 represents a complete, production-ready MinimalAPI framework with zero hardcoded assumptions. The system dynamically adapts to any project structure while providing enterprise-grade features for API development, documentation, and deployment.