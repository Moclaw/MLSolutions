# Moclawr.MinimalAPI

[![NuGet](https://img.shields.io/nuget/v/Moclawr.MinimalAPI.svg)](https://www.nuget.org/packages/Moclawr.MinimalAPI/)

## Overview

Moclawr.MinimalAPI is a powerful library for building clean, structured, and maintainable ASP.NET Core Minimal APIs. It provides a class-based approach to endpoint definition with built-in MediatR integration for implementing the CQRS pattern, making it easy to create scalable API applications with clear separation of concerns.
- Strong typing for requests and responses
- Automatic model binding from various sources (route, query, body, form, header)
- Standardized response handling
- Support for versioning and authorization

## Getting Started

1. Add a reference to the MinimalAPI project in your application.
2. Register the MinimalAPI services in your `Program.cs` file.
3. Create endpoint classes that inherit from `EndpointBase`.
4. Map all endpoints in your `Program.cs` file.

## Usage Example

### Step 1: Register Services

In your `Program.cs` file, register the Minimal API services:

```csharp
// Register Minimal API services
builder.Services.AddMinimalApi(
    typeof(Program).Assembly,  // Endpoints assembly
    typeof(Application).Assembly,  // Handlers assembly
    typeof(Infrastructure).Assembly  // Infrastructure assembly
);

// ... other service registrations

var app = builder.Build();

// Map all endpoints from the assembly
app.MapMinimalEndpoints(typeof(Program).Assembly);
```

### Step 2: Create Endpoint Classes

Create classes that inherit from `EndpointBase<TRequest, TResponse>` for endpoints that return data:

```csharp
public class GetAllTodosEndpoint(IMediator mediator) : EndpointBase<GetAllRequest, GetAllResponse>(mediator)
{
    [HttpGet("api/todos")]
    public override async Task<Response<GetAllResponse>> HandleAsync(GetAllRequest req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
```

Or use `EndpointBase<TRequest>` for endpoints that don't return data:

```csharp
public class DeleteTodoEndpoint(IMediator mediator) : EndpointBase<DeleteRequest>(mediator)
{
    [HttpDelete("api/todos/{id}")]
    public override async Task<Response> HandleAsync(DeleteRequest req, CancellationToken ct)
    {
        return await _mediator.Send(req, ct);
    }
}
```

### Step 3: Create Request Classes

For commands that modify data and return a result:

```csharp
public class CreateRequest : ICommand<CreateResponse>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
```

For commands that don't return data:

```csharp
public class DeleteRequest : ICommand
{
    [FromRoute]
    public int Id { get; set; }
}
```

For queries that return data:

```csharp
public class GetAllRequest : IQueryRequest<GetAllResponse>
{
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
}

public class GetAllResponse
{
    public List<TodoItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
```

### Step 4: Implement Command/Query Handlers

Command handler with response:

```csharp
public class CreateHandler(ITodoRepository repository) : ICommandHandler<CreateRequest, CreateResponse>
{
    private readonly ITodoRepository _repository = repository;

    public async Task<Response<CreateResponse>> Handle(CreateRequest request, CancellationToken cancellationToken)
    {
        var todo = new TodoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        var id = await _repository.AddAsync(todo, cancellationToken);
        await _repository.SaveChangeAsync(cancellationToken);

        return new Response<CreateResponse>
        {
            Data = new CreateResponse { Id = id, Title = todo.Title }
        };
    }
}
```

Command handler without response:

```csharp
public class DeleteHandler(ITodoRepository repository) : ICommandHandler<DeleteRequest>
{
    private readonly ITodoRepository _repository = repository;

    public async Task<Response> Handle(DeleteRequest request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (todo == null)
            return Response.NotFound($"Todo with id {request.Id} not found");

        _repository.Remove(todo);
        await _repository.SaveChangeAsync(cancellationToken);

        return Response.Success();
    }
}
```

Query handler:

```csharp
public class GetAllHandler(ITodoRepository repository) : IQueryHandler<GetAllRequest, GetAllResponse>
{
    private readonly ITodoRepository _repository = repository;

    public async Task<Response<GetAllResponse>> Handle(GetAllRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            request.Search, 
            request.PageIndex, 
            request.PageSize, 
            cancellationToken
        );

        return new Response<GetAllResponse>
        {
            Data = new GetAllResponse
            {
                Items = items.Select(i => new TodoItemDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    IsCompleted = i.IsCompleted
                }).ToList(),
                TotalCount = totalCount
            }
        };
    }
}
```

## Advanced Features

### Model Binding

The framework automatically binds request data from various sources:

- `[FromRoute]` - Bind from route parameters
- `[FromQuery]` - Bind from query string
- `[FromBody]` - Bind from request body (JSON)
- `[FromForm]` - Bind from form data
- `[FromHeader]` - Bind from HTTP headers
- `[FromServices]` - Inject services from DI container

Example:
```csharp
public class UpdateRequest : ICommand<UpdateResponse>
{
    [FromRoute]
    public int Id { get; set; }
    
    [FromBody]
    public string Title { get; set; } = string.Empty;
    
    [FromBody]
    public string? Description { get; set; }
    
    [FromQuery]
    public bool IsCompleted { get; set; }
    
    [FromServices]
    public ICurrentUserService CurrentUser { get; set; } = null!;
}
```

### Response Handling

The `Response` and `Response<T>` classes provide standardized response handling:

```csharp
// Success response with data
return new Response<TodoItemDto>
{
    Data = todoDto,
    Message = "Todo item retrieved successfully"
};

// Success response without data
return Response.Success("Operation completed successfully");

// Error responses
return Response.NotFound("Todo item not found");
return Response.BadRequest("Invalid input data");
return Response.Forbidden("You don't have permission to access this resource");
return Response.Error("An error occurred", StatusCodes.Status500InternalServerError);
```

### Versioning

The library supports API versioning:

```csharp
[HttpGet("api/v{version}/todos")]
public override async Task<Response<GetAllResponse>> HandleAsync(GetAllRequest req, CancellationToken ct)
{
    return await _mediator.Send(req, ct);
}
```

### Authorization

You can apply authorization to endpoints:

```csharp
[HttpPost("api/todos")]
[Authorize(Roles = "Admin,Editor")]
public override async Task<Response<CreateResponse>> HandleAsync(CreateRequest req, CancellationToken ct)
{
    return await _mediator.Send(req, ct);
}
```

## Conclusion

This MinimalAPI library provides a structured approach to building APIs with minimal boilerplate while maintaining the benefits of strong typing, dependency injection, and separation of concerns through the CQRS pattern.
