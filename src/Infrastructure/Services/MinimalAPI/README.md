# Simple Minimal API Library for .NET 9

This library provides a structured approach to creating Minimal APIs in .NET 9 with the following features:

- Controller-like attribute routing
- Built-in MediatR integration
- Support for CQRS pattern
- Simple and clean API definition

## Getting Started

1. Add a reference to the SimpleMinimalAPI project in your application.
2. Register the SimpleMinimalAPI services in your `Program.cs` file.
3. Create endpoint classes and implement your API methods.
4. Map all endpoints in your `Program.cs` file.

## Usage Example

### Step 1: Register Services

In your `Program.cs` file, register the Simple Minimal API services:

```csharp
// Add SimpleMinimalAPI services (including MediatR)
builder.Services.AddSimpleMinimalApi(typeof(Program).Assembly);

// ... other service registrations

var app = builder.Build();

// Map all endpoints from the assembly
app.MapSimpleMinimalEndpoints(typeof(Program).Assembly);
```

### Step 2: Create Endpoint Classes

Create classes that inherit from `EndpointBase` or one of the specialized endpoint classes. You can also use the generic `Endpoint<TRequest, TResponse>` for custom logic:

```csharp
[Route("api/todos")]
public class TodoEndpoint : Endpoint<TodoRequest, TodoResponse>
{
    public override async Task<TodoResponse> HandleAsync(TodoRequest req, CancellationToken ct)
    {
        // Your business logic here
        return new TodoResponse(/* ... */);
    }
}
```

### Step 3: Create MediatR Command/Query Classes

```csharp
// Query with result
public record GetAllTodosQuery() : IRequest<IEnumerable<TodoItem>>;

// Command with result
public record CreateTodoCommand(string Title) : IRequest<int>;

// Command without result
public record DeleteTodoCommand(int Id) : IRequest;
```

### Step 4: Implement MediatR Handlers

```csharp
public class GetAllTodosQueryHandler : IRequestHandler<GetAllTodosQuery, IEnumerable<TodoItem>>
{
    private readonly ITodoRepository _repository;
    public GetAllTodosQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<TodoItem>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
```

## Endpoint<TRequest, TResponse> Usage

- The `Endpoint<TRequest, TResponse>` base class allows you to define endpoints with strong typing for request and response.
- The framework will call `HandleAsync` with the bound request object.
- If your response implements `IResponse`, the HTTP status code will be set from `StatusCode` property.
- The response is serialized to JSON and written to the HTTP response.
- No dependency on custom binders or context classes is required for basic usage.

## Specialized Endpoint Classes

You can still use specialized endpoint classes for CQRS and MediatR integration as shown in the previous examples.
