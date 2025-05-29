# Moclawr.MinimalAPI

[![NuGet](https://img.shields.io/nuget/v/Moclawr.MinimalAPI.svg)](https://www.nuget.org/packages/Moclawr.MinimalAPI/)

## Overview

Moclawr.MinimalAPI provides a class-based approach to ASP.NET Core Minimal APIs with automatic endpoint discovery, MediatR integration, and enhanced OpenAPI documentation. It bridges the gap between minimal APIs and traditional controller-based APIs by offering a structured, object-oriented approach while maintaining the performance benefits of minimal APIs.

## Features

- **Class-Based Endpoints**: Object-oriented endpoint design with base classes
- **Automatic Discovery**: Auto-registration of endpoint classes from assemblies
- **MediatR Integration**: Built-in CQRS pattern support with command/query separation
- **Advanced API Versioning**: Multiple versioning strategies (URL segment, query string, headers, media type)
- **Enhanced OpenAPI Documentation**: Rich SwaggerUI with automatic schema generation
- **Intelligent Request Binding**: Automatic parameter binding from route, query, body, form, and headers
- **Type-Safe Endpoints**: Strongly-typed request and response handling
- **Flexible Response Types**: Support for single items, collections, and custom response wrappers
- **Attribute-Based Configuration**: Declarative endpoint configuration with attributes
- **Custom Authentication**: Built-in authorization and authentication support

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

// Add MinimalAPI with enhanced documentation
builder.Services.AddMinimalApiWithSwaggerUI(
    title: "My API",
    version: "v1",
    description: "Comprehensive API built with MinimalAPI framework"
);

var app = builder.Build();

// Auto-discover and map all endpoints
app.MapMinimalEndpoints(typeof(Program).Assembly);

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
    : ItemEndpointBase<GetUserQuery, UserDto>(mediator)
{
    [HttpGet("/api/users/{id}")]
    public override async Task<Response<UserDto>> HandleAsync(
        GetUserQuery request, CancellationToken ct)
    {
        return await _mediator.Send(request, ct);
    }
}
```

### 3. Define Request Models

```csharp
using MediatR;
using MinimalAPI.Attributes;

public record GetUserQuery : IRequest<Response<UserDto>>
{
    [FromRoute]
    public int Id { get; init; }
}

public record CreateUserRequest : IRequest<Response<UserDto>>
{
    [FromBody]
    public string Name { get; init; } = string.Empty;
    
    [FromBody]
    public string Email { get; init; } = string.Empty;
}
```

## Core Concepts

### Endpoint Base Classes

The framework provides several base classes for different endpoint patterns:

#### SingleEndpointBase<TRequest, TResponse>
For endpoints returning a single item:

```csharp
public class CreateUserEndpoint(IMediator mediator) 
    : SingleEndpointBase<CreateUserCommand, UserDto>(mediator)
{
    [HttpPost("/api/users")]
    public override async Task<Response<UserDto>> HandleAsync(
        CreateUserCommand request, CancellationToken ct)
    {
        return await _mediator.Send(request, ct);
    }
}
```

#### CollectionEndpointBase<TRequest, TResponse>
For endpoints returning collections:

```csharp
public class GetUsersEndpoint(IMediator mediator) 
    : CollectionEndpointBase<GetUsersQuery, UserDto>(mediator)
{
    [HttpGet("/api/users")]
    public override async Task<ResponseCollection<UserDto>> HandleAsync(
        GetUsersQuery request, CancellationToken ct)
    {
        return await _mediator.Send(request, ct);
    }
}
```

#### EndpointBase<TRequest, TResponse, TResponseWrapper>
For custom response types:

```csharp
public class CustomEndpoint(IMediator mediator) 
    : EndpointBase<CustomRequest, CustomData, CustomResponse>(mediator)
{
    [HttpPost("/api/custom")]
    public override async Task<CustomResponse> HandleAsync(
        CustomRequest request, CancellationToken ct)
    {
        return await _mediator.Send(request, ct);
    }
}
```

### Request Binding

The framework automatically binds request data from various sources:

#### Attribute-Based Binding
```csharp
public class UpdateUserCommand
{
    [FromRoute("id")]
    public int UserId { get; set; }
    
    [FromBody]
    public string Name { get; set; }
    
    [FromQuery]
    public bool NotifyUser { get; set; }
    
    [FromHeader("X-Client-Version")]
    public string? ClientVersion { get; set; }
    
    [FromServices]
    public ILogger<UpdateUserCommand> Logger { get; set; }
}
```

#### Smart Default Binding
- **Route parameters**: Automatically bound by name
- **Query parameters**: For GET requests and query strings
- **Request body**: For POST/PUT/PATCH with JSON content
- **Form data**: For multipart/form-data and form submissions
- **Headers**: For custom header values

### API Versioning

Comprehensive versioning support with multiple strategies:

#### Configuration
```csharp
var versioningOptions = new DefaultVersioningOptions
{
    Prefix = "v",
    DefaultVersion = 1,
    SupportedVersions = [1, 2, 3],
    
    // Multiple reading strategies
    ReadingStrategy = VersionReadingStrategy.UrlSegment | 
                     VersionReadingStrategy.QueryString |
                     VersionReadingStrategy.Header,
    
    QueryParameterName = "version",
    VersionHeaderName = "X-API-Version",
    AssumeDefaultVersionWhenUnspecified = true
};

builder.Services.AddMinimalApiWithSwaggerUI(
    versioningOptions: versioningOptions
);
```

#### Version-Specific Endpoints
```csharp
[ApiVersion(1)]
public class GetUsersV1Endpoint : CollectionEndpointBase<GetUsersQuery, UserDto>
{
    [HttpGet("/api/users")]
    public override async Task<ResponseCollection<UserDto>> HandleAsync(...)
    {
        // Version 1 implementation
    }
}

[ApiVersion(2)]
public class GetUsersV2Endpoint : CollectionEndpointBase<GetUsersQueryV2, UserDtoV2>
{
    [HttpGet("/api/users")]
    public override async Task<ResponseCollection<UserDtoV2>> HandleAsync(...)
    {
        // Version 2 implementation with enhanced features
    }
}
```

#### Accessing Versions
```bash
# URL Segment
GET /api/v1/users
GET /api/v2/users

# Query Parameter
GET /api/users?version=1
GET /api/users?version=2

# Header
GET /api/users
X-API-Version: 1

# Media Type
GET /api/users
Accept: application/json;version=1
```

### Enhanced OpenAPI Documentation

#### Rich Documentation Attributes
```csharp
[OpenApiSummary("Create a new user", "Creates a new user account with the provided information")]
[OpenApiParameter("name", "User's full name", required: true, example: "John Doe")]
[OpenApiParameter("email", "User's email address", required: true, example: "john@example.com")]
[OpenApiResponse(201, "User created successfully", typeof(UserDto))]
[OpenApiResponse(400, "Invalid user data provided")]
[OpenApiResponse(409, "User with this email already exists")]
public class CreateUserEndpoint : SingleEndpointBase<CreateUserCommand, UserDto>
{
    [HttpPost("/api/users")]
    public override async Task<Response<UserDto>> HandleAsync(...)
    {
        return await _mediator.Send(request, ct);
    }
}
```

#### Automatic Schema Generation
The framework automatically generates OpenAPI schemas for:
- Request/response types
- Enum values with descriptions
- File upload parameters
- Pagination metadata
- Error response formats

#### Feature-Based Tagging
Endpoints are automatically tagged based on namespace structure:

```
Namespace: MyApp.Features.Users.Commands
Generated Tags: ["Users", "Users Commands"]

Namespace: MyApp.Features.Orders.Queries  
Generated Tags: ["Orders", "Orders Queries"]
```

### File Upload Support

#### Single File Upload
```csharp
public class UploadFileCommand
{
    [FromForm]
    public IFormFile File { get; set; }
    
    [FromForm]
    public string Description { get; set; }
}

public class UploadFileEndpoint(IMediator mediator)
    : SingleEndpointBase<UploadFileCommand, FileDto>(mediator)
{
    [HttpPost("/api/files")]
    public override async Task<Response<FileDto>> HandleAsync(...)
    {
        return await _mediator.Send(request, ct);
    }
}
```

#### Multiple File Upload
```csharp
public class UploadFilesCommand
{
    [FromForm]
    public List<IFormFile> Files { get; set; }
    
    [FromForm]
    public string Folder { get; set; }
}
```

### Authentication and Authorization

#### Endpoint-Level Security
```csharp
[Authorize("AdminPolicy")]
public class DeleteUserEndpoint : SingleEndpointBase<DeleteUserCommand, Response>
{
    [HttpDelete("/api/users/{id}")]
    public override async Task<Response> HandleAsync(...)
    {
        return await _mediator.Send(request, ct);
    }
}
```

#### Custom Authorization
```csharp
[RequireRole("Admin", "Manager")]
[RequireClaim("permission", "users:delete")]
public class DeleteUserEndpoint : SingleEndpointBase<DeleteUserCommand, Response>
{
    // Implementation
}
```

## Advanced Features

### Custom Endpoint Filters

```csharp
public class LoggingEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<LoggingEndpointFilter>>();
        
        logger.LogInformation("Executing endpoint: {Endpoint}", 
            context.HttpContext.Request.Path);
        
        var result = await next(context);
        
        logger.LogInformation("Completed endpoint: {Endpoint}", 
            context.HttpContext.Request.Path);
        
        return result;
    }
}
```

### Response Caching

```csharp
[ResponseCache(Duration = 300)] // Cache for 5 minutes
public class GetUserEndpoint : SingleEndpointBase<GetUserQuery, UserDto>
{
    [HttpGet("/api/users/{id}")]
    public override async Task<Response<UserDto>> HandleAsync(...)
    {
        return await _mediator.Send(request, ct);
    }
}
```

### Rate Limiting

```csharp
[RateLimit("UserEndpoints", Window = "1m", Limit = 60)]
public class CreateUserEndpoint : SingleEndpointBase<CreateUserCommand, UserDto>
{
    [HttpPost("/api/users")]
    public override async Task<Response<UserDto>> HandleAsync(...)
    {
        return await _mediator.Send(request, ct);
    }
}
```

## Integration Examples

### With Entity Framework Core
```csharp
public class GetUsersHandler(IQueryRepository<User, int> repository)
    : IRequestHandler<GetUsersQuery, ResponseCollection<UserDto>>
{
    public async Task<ResponseCollection<UserDto>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await repository.GetPagedListAsync<UserDto>(
            request.Page,
            request.PageSize,
            filter: u => u.IsActive,
            orderBy: q => q.OrderBy(u => u.Name),
            cancellationToken: cancellationToken
        );

        return ResponseHelper.CreateSuccessCollection(users);
    }
}
```

### With Caching
```csharp
public class GetUserHandler(
    IQueryRepository<User, int> repository,
    ICacheService cache)
    : IRequestHandler<GetUserQuery, Response<UserDto>>
{
    public async Task<Response<UserDto>> Handle(
        GetUserQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"user:{request.Id}";
        
        var cachedUser = await cache.GetAsync<UserDto>(cacheKey);
        if (cachedUser != null)
        {
            return ResponseHelper.CreateSuccess(cachedUser);
        }

        var user = await repository.GetByIdAsync<UserDto>(
            request.Id, cancellationToken: cancellationToken);
        
        if (user != null)
        {
            await cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(15));
        }

        return ResponseHelper.CreateSuccess(user);
    }
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Uses extension methods and utilities for enhanced functionality
- **Moclawr.Shared**: Integrates with response models and entity interfaces
- **Moclawr.Host**: Perfect companion for global exception handling and infrastructure
- **Moclawr.EfCore** and **Moclawr.MongoDb**: Works with repository patterns for data access
- **Moclawr.Services.Caching**: Enables response caching for API endpoints
- **Moclawr.Services.External**: Integrates external services in endpoint handlers
- **Moclawr.Services.Autofac**: Enhanced dependency injection for endpoint registration

## Requirements

- .NET 9.0 or higher
- MediatR 12.4.1 or higher
- Microsoft.AspNetCore.OpenApi 9.0.5 or higher
- Swashbuckle.AspNetCore 7.2.0 or higher

## License

This package is licensed under the MIT License.