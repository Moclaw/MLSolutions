# Moclawr.MinimalAPI

[![NuGet](https://img.shields.io/nuget/v/Moclawr.MinimalAPI.svg)](https://www.nuget.org/packages/Moclawr.MinimalAPI/)

## Overview

Moclawr.MinimalAPI is a powerful library for building clean, structured, and maintainable ASP.NET Core Minimal APIs for .NET 9. It provides a class-based approach to endpoint definition with built-in MediatR integration for implementing the CQRS pattern, making it easy to create scalable API applications with clear separation of concerns.

Key Features:
- Strong typing for requests and responses
- **Smart automatic parameter binding with intelligent defaults**
- **Enhanced OpenAPI documentation with automatic parameter detection**
- Standardized response handling
- Support for versioning and authorization
- **Command/Query pattern support with automatic binding defaults**

## Getting Started

1. Add a reference to the MinimalAPI project in your application.
2. Register the MinimalAPI services in your `Program.cs` file.
3. Create endpoint classes that inherit from `EndpointBase`.
4. Map all endpoints in your `Program.cs` file.

## Usage Example

### Step 1: Register Services

In your `Program.cs` file, register the Minimal API services with enhanced OpenAPI:

```csharp
// Register Minimal API services with OpenAPI documentation
builder.Services.AddMinimalApiWithOpenApi(
    title: "Todo API",
    version: "v1", 
    description: "Comprehensive API for Todo Management",
    assemblies: [
        typeof(Program).Assembly,  // Endpoints assembly
        typeof(Application.Register).Assembly,  // Handlers assembly
        typeof(Infrastructure.Register).Assembly  // Infrastructure assembly
    ]
);

// ... other service registrations

var app = builder.Build();

// Enable OpenAPI in development
if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiOpenApi();
}

// Map all endpoints from the assembly
app.MapMinimalEndpoints(typeof(Program).Assembly);
```

### Step 2: Create Request Classes with Smart Defaults

#### Command Requests (Default to Request Body)

Commands automatically use request body for all properties by default:

```csharp
public class CreateTodoRequest : ICommand<CreateTodoResponse>
{
    // These properties automatically go to request body for POST requests
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public List<int> TagIds { get; set; } = new();
    
    // Override default behavior with attributes when needed
    [FromRoute]
    public int? ParentId { get; set; }  // This comes from route parameter
}

public class UpdateTodoRequest : ICommand<UpdateTodoResponse>
{
    [FromRoute]  // Route parameters need explicit attribute
    public int Id { get; set; }
    
    // These automatically go to request body (smart default for commands)
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
```

#### Query Requests (Default to Query Parameters)

Queries automatically use query parameters for all properties by default:

```csharp
public class GetAllTodosRequest : IQueryCollectionRequest<TodoItemDto>
{
    // These properties automatically become query parameters for GET requests
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string OrderBy { get; set; } = "Id";
    public bool IsAscending { get; set; } = true;
    public List<int>? CategoryIds { get; set; }
    
    // Override default behavior when needed
    [FromHeader]
    public string? UserAgent { get; set; }  // This comes from header
}

public class GetTodoByIdRequest : IQueryRequest<TodoItemDto>
{
    [FromRoute]  // Route parameters need explicit attribute
    public int Id { get; set; }
    
    // These automatically become query parameters (smart default for queries)
    public bool IncludeTags { get; set; } = false;
    public bool IncludeCategory { get; set; } = false;
}
```

### Step 3: Create Endpoint Classes with OpenAPI Documentation

#### Command Endpoints with Auto-Generated Documentation

```csharp
[EndpointSummary("Create a new todo", Description = "Creates a new todo item with the provided details")]
[OpenApiResponse(201, ResponseType = typeof(Response<CreateTodoResponse>), Description = "Todo created successfully")]
[OpenApiResponse(400, Description = "Invalid request data")]
public class CreateTodoEndpoint(IMediator mediator)
    : SingleEndpointBase<CreateTodoRequest, CreateTodoResponse>(mediator)
{
    [HttpPost("api/todos")]
    public override async Task<Response<CreateTodoResponse>> HandleAsync(
        CreateTodoRequest req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
```

The OpenAPI documentation will automatically show:
- `Title`, `Description`, `CategoryId`, `TagIds` as request body parameters
- `ParentId` as a route parameter (due to `[FromRoute]` attribute)

#### Query Endpoints with Auto-Generated Documentation

```csharp
[EndpointSummary("Get all todos", Description = "Retrieves a paginated list of todos with optional filtering")]
[OpenApiResponse(200, ResponseType = typeof(ResponseCollection<TodoItemDto>), Description = "Successfully retrieved todos")]
[OpenApiResponse(400, Description = "Invalid request parameters")]
public class GetAllTodosEndpoint(IMediator mediator)
    : CollectionEndpointBase<GetAllTodosRequest, TodoItemDto>(mediator)
{
    [HttpGet("api/todos")]
    public override async Task<ResponseCollection<TodoItemDto>> HandleAsync(
        GetAllTodosRequest req,
        CancellationToken ct
    )
    {
        return await mediator.Send(req, ct);
    }
}
```

The OpenAPI documentation will automatically show:
- `Search`, `PageIndex`, `PageSize`, `OrderBy`, `IsAscending`, `CategoryIds` as query parameters
- `UserAgent` as a header parameter (due to `[FromHeader]` attribute)

### Step 4: Advanced Parameter Binding with Custom Attributes

You can override the smart defaults using explicit binding attributes:

```csharp
public class AdvancedRequest : ICommand<AdvancedResponse>
{
    [FromRoute]
    public int Id { get; set; }
    
    [FromQuery]  // Override: Force this to be a query parameter instead of body
    public bool ForceUpdate { get; set; }
    
    [FromHeader("X-User-Id")]
    public string? UserId { get; set; }
    
    // These still go to request body (default for commands)
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    [FromServices]
    public ICurrentUserService CurrentUser { get; set; } = null!;
}
```

## Advanced Features

### Smart Parameter Binding Defaults

The framework provides intelligent defaults based on request type:

#### Command Requests (`ICommand`, `ICommand<T>`)
- **Default**: All properties go to request body (perfect for POST, PUT, PATCH)
- **Override**: Use `[FromRoute]`, `[FromQuery]`, `[FromHeader]` for exceptions
- **Auto-detected**: Properties named `Id`, `TodoId` automatically become route parameters

#### Query Requests (`IQueryRequest`, `IQueryCollectionRequest`)
- **Default**: All properties become query parameters (perfect for GET requests)
- **Override**: Use `[FromRoute]`, `[FromHeader]`, `[FromBody]` for exceptions
- **Auto-detected**: Properties named `Id`, `TodoId` automatically become route parameters

### Enhanced OpenAPI Documentation

The library automatically generates comprehensive OpenAPI documentation:

#### Automatic Parameter Detection
```csharp
[EndpointSummary("Update todo status")]
public class UpdateTodoStatusEndpoint : SingleEndpointBase<UpdateStatusRequest, UpdateStatusResponse>
{
    // OpenAPI automatically detects:
    // - Id as route parameter
    // - IsCompleted as request body parameter
    // - LastModified as request body parameter
}
```

#### Custom Parameter Documentation
```csharp
[OpenApiParameter("search", typeof(string), Description = "Search term for filtering todos", Example = "grocery")]
[OpenApiParameter("pageSize", typeof(int), Description = "Number of items per page", Example = 10)]
[OpenApiResponse(200, Description = "Search completed successfully")]
public class SearchTodosEndpoint : CollectionEndpointBase<SearchRequest, TodoItemDto>
{
    // Custom documentation with examples and detailed descriptions
}
```

### Model Binding Attributes

Available binding attributes for fine-grained control:

```csharp
public class ComprehensiveRequest : ICommand<ComprehensiveResponse>
{
    [FromRoute("todoId")]           // Bind from route parameter 'todoId'
    public int Id { get; set; }
    
    [FromQuery("q")]                // Bind from query parameter 'q'
    public string? SearchTerm { get; set; }
    
    [FromHeader("Authorization")]   // Bind from Authorization header
    public string? AuthToken { get; set; }
    
    [FromBody]                      // Explicitly from request body (default for commands)
    public TodoUpdateData Data { get; set; } = new();
    
    [FromForm]                      // Bind from form data
    public IFormFile? Attachment { get; set; }
    
    [FromServices]                  // Inject from DI container
    public ILogger<ComprehensiveRequest> Logger { get; set; } = null!;
}
```

### Response Handling

The library provides standardized response handling:

```csharp
// Success response with data
return new Response<TodoItemDto>(
    IsSuccess: true,
    StatusCode: 200,
    Message: "Todo retrieved successfully",
    Data: todoDto
);

// Collection response
return new ResponseCollection<TodoItemDto>(
    IsSuccess: true,
    StatusCode: 200,
    Message: "Todos retrieved successfully",
    Data: todos
);

// Error responses
return new Response(false, 404, "Todo not found");
return new Response(false, 400, "Invalid request data");
```

### Versioning Support

```csharp
// Configure versioning in Program.cs
services.Configure<VersioningOptions>(options => 
{
    options.Prefix = "v";
    options.DefaultVersion = 1;
});

// Use versioning in endpoints
[HttpGet("api/v{version}/todos")]
public override async Task<ResponseCollection<TodoItemDto>> HandleAsync(GetAllRequest req, CancellationToken ct)
{
    return await _mediator.Send(req, ct);
}
```

## Key Benefits

1. **Zero Configuration**: Smart defaults work out of the box
2. **Type Safety**: Strong typing for all requests and responses
3. **Automatic Documentation**: OpenAPI docs generated automatically
4. **Flexible Override**: Easy to customize when needed
5. **CQRS Pattern**: Built-in support for Command/Query separation
6. **Clean Architecture**: Minimal boilerplate, maximum productivity

## Conclusion

This MinimalAPI library provides an intuitive approach to building APIs with intelligent defaults that follow REST conventions, while maintaining full flexibility for customization. The smart parameter binding reduces boilerplate code and the automatic OpenAPI documentation ensures your APIs are always well-documented.