# Moclawr.MinimalAPI

[![NuGet](https://img.shields.io/nuget/v/Moclawr.MinimalAPI.svg)](https://www.nuget.org/packages/Moclawr.MinimalAPI/)

## Overview

Moclawr.MinimalAPI is a powerful library for building clean, structured, and maintainable ASP.NET Core Minimal APIs for .NET 9. It provides a class-based approach to endpoint definition with built-in MediatR integration for implementing the CQRS pattern, making it easy to create scalable API applications with clear separation of concerns.

Key Features:
- Strong typing for requests and responses
- **Smart automatic parameter binding with intelligent defaults**
- **Enhanced OpenAPI documentation with automatic parameter detection**
- **Rich SwaggerUI integration with custom styling and functionality**
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

#### Option A: OpenAPI Only
```csharp
// Register Minimal API services with OpenAPI documentation only
builder.Services.AddMinimalApiWithOpenApi(
    title: "Todo API",
    version: "v1", 
    description: "Comprehensive API for Todo Management",
    assemblies: [
        typeof(Program).Assembly,
        typeof(Application.Register).Assembly,
        typeof(Infrastructure.Register).Assembly
    ]
);
```

#### Option B: Complete Documentation with SwaggerUI (Recommended)
```csharp
// Register Minimal API services with enhanced SwaggerUI
builder.Services.AddMinimalApiWithSwaggerUI(
    title: "Todo API",
    version: "v1",
    description: "Comprehensive API for Todo Management with CRUD operations, built using MinimalAPI framework with MediatR and CQRS pattern",
    contactName: "Your Development Team",
    contactEmail: "dev@yourcompany.com",
    contactUrl: "https://yourcompany.com/contact",
    licenseName: "MIT",
    licenseUrl: "https://opensource.org/licenses/MIT",
    assemblies: [
        typeof(Program).Assembly,
        typeof(Application.Register).Assembly,
        typeof(Infrastructure.Register).Assembly
    ]
);

// ... other service registrations

var app = builder.Build();

// Enable complete documentation (OpenAPI + SwaggerUI)
if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiDocs(
        swaggerRoutePrefix: "docs",  // Custom route: /docs
        enableTryItOut: true,
        enableDeepLinking: true,
        enableFilter: true
    );
}

// Map all endpoints from the assembly
app.MapMinimalEndpoints(typeof(Program).Assembly);
```

### Step 2: SwaggerUI-Specific Features

#### Enhanced UI Configuration
```csharp
// Advanced SwaggerUI configuration
app.UseMinimalApiSwaggerUI(
    routePrefix: "api-docs",           // Custom route prefix
    enableTryItOut: true,              // Enable "Try it out" by default
    enableDeepLinking: true,           // Enable deep linking
    enableFilter: true,                // Enable endpoint filtering
    enableValidator: false,            // Disable validator
    docExpansion: DocExpansion.List,   // Expand operations list
    defaultModelRendering: ModelRendering.Example, // Show examples
    persistAuthorization: true        // Remember auth tokens
);
```

#### Custom Styling and Assets
The SwaggerUI comes with enhanced styling and custom functionality:

**Features:**
- **Custom CSS**: Modern gradient header, enhanced colors, better readability
- **Copy Buttons**: Automatic copy buttons for code blocks
- **Enhanced Buttons**: Styled "Try it out" and "Execute" buttons
- **Responsive Design**: Better mobile and tablet experience
- **Custom JavaScript**: Enhanced functionality and user experience

### Step 3: Create Request Classes with Smart Defaults

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

### Step 4: Create Endpoint Classes with Enhanced Documentation

#### Command Endpoints with Rich SwaggerUI Documentation

```csharp
[OpenApiSummary("Create a new todo", 
    Description = "Creates a new todo item with the provided details. Supports categorization and tagging.",
    Tags = new[] { "Todo Management", "CRUD Operations" })]
[OpenApiResponse(201, ResponseType = typeof(Response<CreateTodoResponse>), 
    Description = "Todo created successfully with generated ID")]
[OpenApiResponse(400, Description = "Invalid request data - validation failed")]
[OpenApiResponse(409, Description = "Todo with similar title already exists")]
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

The enhanced SwaggerUI will automatically show:
- **Rich Documentation**: Detailed descriptions, examples, and response codes
- **Interactive Examples**: "Try it out" functionality with sample data
- **Request Body Schema**: Automatically generated from `CreateTodoRequest`
- **Response Examples**: Sample responses for different status codes
- **Parameter Validation**: Required/optional field indicators

#### Query Endpoints with Advanced Filtering UI

```csharp
[OpenApiSummary("Get all todos with filtering", 
    Description = "Retrieves a paginated list of todos with advanced filtering, sorting, and search capabilities")]
[OpenApiParameter("search", typeof(string), Description = "Search term for filtering todos by title or description", Example = "grocery")]
[OpenApiParameter("pageSize", typeof(int), Description = "Number of items per page (max 100)", Example = 10)]
[OpenApiParameter("categoryIds", typeof(List<int>), Description = "Filter by specific category IDs", Example = new[] { 1, 2, 3 })]
[OpenApiResponse(200, ResponseType = typeof(ResponseCollection<TodoItemDto>), 
    Description = "Successfully retrieved todos with pagination metadata")]
[OpenApiResponse(400, Description = "Invalid pagination or filter parameters")]
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

## SwaggerUI Advanced Features

### Custom Theming and Branding

The SwaggerUI comes with a professional, modern theme:

```css
/* Automatic features included */
- Gradient header with company branding
- Enhanced operation blocks with status-specific colors
- Modern button styling with hover effects
- Improved typography and spacing
- Mobile-responsive design
- Dark mode friendly colors
```

### Enhanced Functionality

**Automatic Features:**
- **Copy to Clipboard**: All code blocks get copy buttons
- **Request Duration Display**: Shows API response times
- **Persistent Authorization**: Remembers auth tokens across sessions
- **Deep Linking**: Direct links to specific operations
- **Advanced Filtering**: Search through endpoints
- **Example Generation**: Automatic request/response examples

### Security Integration

```csharp
// SwaggerUI automatically detects and displays security requirements
[Authorize] // Automatically adds security icon and requirements
public class SecureEndpoint : SingleEndpointBase<SecureRequest, SecureResponse>
{
    // Security requirements automatically documented
}
```

### Custom Examples and Documentation

```csharp
[OpenApiParameter("userId", typeof(int), 
    Description = "Unique identifier for the user", 
    Example = 12345,
    Required = true)]
[OpenApiResponse(200, 
    ResponseType = typeof(UserProfileResponse),
    Description = "User profile retrieved successfully")]
public class GetUserProfileEndpoint : SingleEndpointBase<GetUserRequest, UserProfileResponse>
{
    // Rich documentation with examples automatically displayed in SwaggerUI
}
```

## Configuration Options

### Basic Setup (OpenAPI Only)
```csharp
// Minimal setup for basic OpenAPI documentation
builder.Services.AddMinimalApiWithOpenApi("My API", "v1", "API Description");

app.UseMinimalApiOpenApi(); // Only enables OpenAPI endpoint
```

### Full Setup (OpenAPI + SwaggerUI)
```csharp
// Complete setup with enhanced SwaggerUI
builder.Services.AddMinimalApiWithSwaggerUI(
    title: "My API",
    version: "v2.0",
    description: "Comprehensive API with full documentation",
    contactName: "API Support Team",
    contactEmail: "support@company.com",
    licenseName: "MIT"
);

app.UseMinimalApiDocs(
    swaggerRoutePrefix: "documentation",
    enableTryItOut: true,
    enableFilter: true
);
```

### Production Considerations

```csharp
// SwaggerUI only in development/staging
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseMinimalApiDocs();
}

// OpenAPI can be enabled in production for API clients
app.UseMinimalApiOpenApi(); // Always available for API consumers
```