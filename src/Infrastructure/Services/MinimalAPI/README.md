# Moclawr.MinimalAPI

[![NuGet](https://img.shields.io/nuget/v/Moclawr.MinimalAPI.svg)](https://www.nuget.org/packages/Moclawr.MinimalAPI/)

## Overview

Moclawr.MinimalAPI provides a class-based approach to ASP.NET Core Minimal APIs with automatic endpoint discovery, MediatR integration, and enhanced OpenAPI documentation. It bridges the gap between minimal APIs and traditional controller-based APIs by offering a structured, object-oriented approach while maintaining the performance benefits of minimal APIs.

## Key Features

- **Class-Based Endpoints**: Object-oriented endpoint design with base classes
- **Automatic Discovery**: Auto-registration of endpoint classes from assemblies
- **MediatR Integration**: Built-in CQRS pattern support with command/query separation
- **Advanced API Versioning**: Multiple versioning strategies (URL segment, query string, headers, media type)
- **Enhanced OpenAPI Documentation**: Rich SwaggerUI with automatic schema generation
- **Unique Operation IDs**: GUID-based operation IDs prevent duplicate endpoint names
- **Intelligent Request Binding**: Automatic parameter binding from route, query, body, form, and headers
- **Type-Safe Endpoints**: Strongly-typed request and response handling
- **Flexible Response Types**: Support for single items, collections, and custom response wrappers
- **Attribute-Based Configuration**: Declarative endpoint configuration with attributes
- **Custom Authentication**: Built-in authorization and authentication support

## Latest Updates (v2.1.2)

- ✅ **Fixed Duplicate Endpoint Names**: Implemented GUID-based unique operation IDs
- ✅ **Enhanced SwaggerUI**: Improved documentation with versioned endpoints
- ✅ **Better Error Handling**: Comprehensive error responses and validation
- ✅ **Performance Optimizations**: Faster endpoint discovery and registration

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

// Add MinimalAPI with enhanced documentation and versioning
var versioningOptions = new DefaultVersioningOptions
{
    SupportedVersions = [1, 2],
    ReadingStrategy = VersionReadingStrategy.UrlSegment | VersionReadingStrategy.Header
};

builder.Services.AddMinimalApiWithSwaggerUI(
    title: "My API",
    version: "v1",
    description: "Comprehensive API built with MinimalAPI framework",
    versioningOptions: versioningOptions,
    typeof(Program).Assembly
);

var app = builder.Build();

// Auto-discover and map all endpoints
app.MapMinimalEndpoints(versioningOptions, typeof(Program).Assembly);

// Enable documentation
if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiDocs();
}

app.Run();
```

### 2. Create Your First Endpoint

```csharp
using MinimalAPI.Endpoints;
using MinimalAPI.Attributes;
using MediatR;

[OpenApiSummary("Get user by ID", "Retrieves a specific user by their unique identifier")]
[ApiVersion(1)]
public class GetUserEndpoint(IMediator mediator) 
    : SingleEndpointBase<GetUserQuery, UserDto>(mediator)
{
    [HttpGet("/api/v1/users/{id}")]
    public override async Task<Response<UserDto>> HandleAsync(
        GetUserQuery request, CancellationToken ct)
    {
        return await _mediator.Send(request, ct);
    }
}
```

### 3. Define Request Models with Smart Binding

```csharp
using MediatR;
using MinimalAPI.Attributes;

public record GetUserQuery : IRequest<Response<UserDto>>
{
    [FromRoute("id")]
    public int UserId { get; init; }
    
    [FromQuery]
    public bool IncludeDetails { get; init; } = false;
    
    [FromHeader("X-Client-Version")]
    public string? ClientVersion { get; init; }
}

public record CreateUserRequest : IRequest<Response<UserDto>>
{
    [FromBody]
    public string Name { get; init; } = string.Empty;
    
    [FromBody]
    public string Email { get; init; } = string.Empty;
    
    [FromQuery]
    public bool SendWelcomeEmail { get; init; } = true;
}
```

## Unique Operation IDs

The framework automatically generates unique operation IDs using a combination of readable prefixes and deterministic GUIDs:

```
Operation ID Format: {FeatureName}_{EndpointName}_{HttpMethod}_{GUID}
Examples:
- Users_GetAll_GET_a1b2c3d4e5f6789012345678901234ab
- Orders_Create_POST_f9e8d7c6b5a4321098765432109876fe
```

This ensures no duplicate endpoint names across your entire API, even with similar endpoint structures.

## Advanced Versioning

### Multiple Version Support

```csharp
[ApiVersion(1)]
public class GetUsersV1Endpoint : CollectionEndpointBase<GetUsersQuery, UserDtoV1>
{
    [HttpGet("/api/v1/users")]
    public override async Task<ResponseCollection<UserDtoV1>> HandleAsync(...)
    {
        // Version 1 implementation
    }
}

[ApiVersion(2)]
public class GetUsersV2Endpoint : CollectionEndpointBase<GetUsersQueryV2, UserDtoV2>
{
    [HttpGet("/api/v2/users")]
    public override async Task<ResponseCollection<UserDtoV2>> HandleAsync(...)
    {
        // Version 2 with enhanced features
    }
}
```

### Multiple Reading Strategies

```csharp
// URL Segment: GET /api/v1/users
// Query Parameter: GET /api/users?version=1
// Header: GET /api/users (with X-API-Version: 1)
// Media Type: GET /api/users (with Accept: application/json;version=1)
```

## Enhanced Documentation Features

### Automatic Feature-Based Tagging

Endpoints are automatically grouped by namespace structure:

```
Namespace: MyApp.Features.Users.Commands → Tag: "Users Commands"
Namespace: MyApp.Features.Orders.Queries → Tag: "Orders Queries"
```

### Rich OpenAPI Attributes

```csharp
[OpenApiSummary("Create user account", "Creates a new user with validation")]
[OpenApiParameter("name", "User's full name", required: true, example: "John Doe")]
[OpenApiResponse(201, "User created successfully", typeof(UserDto))]
[OpenApiResponse(400, "Invalid user data")]
[ApiVersion(1)]
public class CreateUserEndpoint : SingleEndpointBase<CreateUserCommand, UserDto>
{
    [HttpPost("/api/v1/users")]
    public override async Task<Response<UserDto>> HandleAsync(...)
    {
        return await _mediator.Send(request, ct);
    }
}
```

## File Upload Support

### Single and Multiple Files

```csharp
public class UploadFilesCommand
{
    [FromForm]
    public List<IFormFile> Files { get; set; } = [];
    
    [FromForm]
    public string Description { get; set; } = string.Empty;
    
    [FromQuery]
    public string Folder { get; set; } = "uploads";
}

public class UploadFilesEndpoint(IMediator mediator)
    : SingleEndpointBase<UploadFilesCommand, List<FileDto>>(mediator)
{
    [HttpPost("/api/v1/files/upload")]
    public override async Task<Response<List<FileDto>>> HandleAsync(...)
    {
        return await _mediator.Send(request, ct);
    }
}
```

## Integration Examples

### With Entity Framework and Caching

```csharp
public class GetUsersHandler(
    IQueryRepository<User, int> repository,
    ICacheService cache,
    ILogger<GetUsersHandler> logger)
    : IRequestHandler<GetUsersQuery, ResponseCollection<UserDto>>
{
    public async Task<ResponseCollection<UserDto>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"users:page:{request.Page}:size:{request.PageSize}";
        
        var cachedUsers = await cache.GetAsync<PagedList<UserDto>>(cacheKey);
        if (cachedUsers != null)
        {
            logger.LogInformation("Retrieved users from cache");
            return ResponseHelper.CreateSuccessCollection(cachedUsers);
        }

        var users = await repository.GetPagedListAsync<UserDto>(
            request.Page,
            request.PageSize,
            filter: u => u.IsActive,
            orderBy: q => q.OrderBy(u => u.Name),
            cancellationToken: cancellationToken
        );

        await cache.SetAsync(cacheKey, users, TimeSpan.FromMinutes(15));
        
        return ResponseHelper.CreateSuccessCollection(users);
    }
}
```

## Requirements

- .NET 9.0 or higher
- MediatR 12.2.0 or higher
- Microsoft.AspNetCore.OpenApi 9.0.0 or higher
- Swashbuckle.AspNetCore 8.1.2 or higher

## Integration with Moclawr Ecosystem

This package works seamlessly with:

- **Moclawr.Core**: Extension methods and utilities
- **Moclawr.Shared**: Response models and entity interfaces
- **Moclawr.Host**: Global exception handling and infrastructure
- **Moclawr.EfCore/MongoDb**: Repository patterns for data access
- **Moclawr.Services.Caching**: Response caching for API endpoints
- **Moclawr.Services.External**: External service integration
- **Moclawr.Services.Autofac**: Enhanced dependency injection

## License

This package is licensed under the MIT License.

---

**Note**: Version 2.1.2 introduces GUID-based operation IDs that solve the "Duplicate endpoint name" issue definitively. All endpoints now have globally unique identifiers while maintaining human-readable prefixes for better developer experience.