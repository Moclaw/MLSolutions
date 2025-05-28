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

## Real-World Use Cases

### Use Case 1: E-Commerce Product Catalog API

Perfect for building scalable product management APIs with rich filtering and search capabilities:

```csharp
// Product search with advanced filtering
public class SearchProductsRequest : IQueryCollectionRequest<ProductDto>
{
    public string? SearchTerm { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<string>? Tags { get; set; }
    public bool InStock { get; set; } = true;
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;
}

[OpenApiSummary("Search products with advanced filtering")]
[OpenApiParameter("searchTerm", typeof(string), Description = "Search in product name and description")]
[OpenApiParameter("minPrice", typeof(decimal), Description = "Minimum price filter", Example = 10.00)]
[OpenApiParameter("maxPrice", typeof(decimal), Description = "Maximum price filter", Example = 100.00)]
public class SearchProductsEndpoint(IMediator mediator) 
    : CollectionEndpointBase<SearchProductsRequest, ProductDto>(mediator)
{
    [HttpGet("api/products/search")]
    public override async Task<ResponseCollection<ProductDto>> HandleAsync(
        SearchProductsRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
}
```

### Use Case 2: User Management System

Ideal for authentication and user profile management with robust validation:

```csharp
// User registration with validation
public class RegisterUserRequest : ICommand<RegisterUserResponse>
{
    [EmailAddress, Required]
    public string Email { get; set; } = string.Empty;
    
    [MinLength(8), Required]
    public string Password { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public List<string> Preferences { get; set; } = new();
}

[OpenApiSummary("Register a new user account")]
[OpenApiResponse(201, typeof(Response<RegisterUserResponse>), "User registered successfully")]
[OpenApiResponse(409, Description = "Email already exists")]
[OpenApiResponse(400, Description = "Validation failed")]
public class RegisterUserEndpoint(IMediator mediator)
    : SingleEndpointBase<RegisterUserRequest, RegisterUserResponse>(mediator)
{
    [HttpPost("api/auth/register")]
    public override async Task<Response<RegisterUserResponse>> HandleAsync(
        RegisterUserRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
}
```

### Use Case 3: Real-Time Dashboard API

Great for building analytics and monitoring dashboards with real-time data:

```csharp
// Dashboard metrics with date range filtering
public class GetDashboardMetricsRequest : IQueryRequest<DashboardMetricsDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public List<string>? MetricTypes { get; set; }
    public DashboardView View { get; set; } = DashboardView.Summary;
    
    [FromHeader("X-Organization-Id")]
    public string? OrganizationId { get; set; }
}

[OpenApiSummary("Get dashboard metrics and analytics")]
[OpenApiParameter("startDate", typeof(DateTime), Description = "Start date for metrics (ISO 8601)")]
[OpenApiParameter("view", typeof(DashboardView), Description = "Dashboard view type", Example = DashboardView.Detailed)]
[Authorize(Policy = "DashboardAccess")]
public class GetDashboardMetricsEndpoint(IMediator mediator)
    : SingleEndpointBase<GetDashboardMetricsRequest, DashboardMetricsDto>(mediator)
{
    [HttpGet("api/dashboard/metrics")]
    public override async Task<Response<DashboardMetricsDto>> HandleAsync(
        GetDashboardMetricsRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
}
```

## Complete Setup Example

### Step 1: Register Services in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Minimal API services with enhanced SwaggerUI
builder.Services.AddMinimalApiWithSwaggerUI(
    title: "MLSolutions API",
    version: "v2.1.0",
    description: "Enterprise-grade API built with Moclawr.MinimalAPI framework featuring CQRS, MediatR integration, and comprehensive OpenAPI documentation",
    contactName: "MLSolutions Development Team",
    contactEmail: "dev@mlsolutions.com",
    contactUrl: "https://mlsolutions.com/support",
    licenseName: "MIT License",
    licenseUrl: "https://opensource.org/licenses/MIT",
    assemblies: [
        typeof(Program).Assembly,
        typeof(Application.Register).Assembly,
        typeof(Infrastructure.Register).Assembly,
        typeof(Domain.Register).Assembly
    ]
);

// Add authentication and authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { /* JWT config */ });
builder.Services.AddAuthorization();

// Register MediatR and other services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(Application.Register).Assembly));

var app = builder.Build();

// Configure middleware pipeline
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger documentation in development
if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiDocs(
        swaggerRoutePrefix: "api-docs",
        enableTryItOut: true,
        enableDeepLinking: true,
        enableFilter: true,
        enableValidator: true
    );
}

// Map all minimal API endpoints
app.MapMinimalEndpoints(
    typeof(Program).Assembly,
    typeof(Application.Register).Assembly
);

app.Run();
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

### Step 3: Advanced Use Case Implementation

#### File Upload with Progress Tracking

```csharp
public class UploadFileRequest : ICommand<UploadFileResponse>
{
    [FromForm]
    public IFormFile File { get; set; } = null!;
    
    [FromForm]
    public string? Description { get; set; }
    
    [FromForm]
    public List<string> Tags { get; set; } = new();
    
    [FromRoute]
    public int ProjectId { get; set; }
}

[OpenApiSummary("Upload file with metadata")]
[OpenApiResponse(201, typeof(Response<UploadFileResponse>), "File uploaded successfully")]
[OpenApiResponse(413, Description = "File too large")]
[OpenApiResponse(415, Description = "Unsupported file type")]
[Authorize]
public class UploadFileEndpoint(IMediator mediator)
    : SingleEndpointBase<UploadFileRequest, UploadFileResponse>(mediator)
{
    [HttpPost("api/projects/{projectId}/files")]
    [RequestSizeLimit(50_000_000)] // 50MB limit
    public override async Task<Response<UploadFileResponse>> HandleAsync(
        UploadFileRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
}
```

#### Bulk Operations with Transaction Support

```csharp
public class BulkUpdateProductsRequest : ICommand<BulkUpdateProductsResponse>
{
    public List<ProductUpdateDto> Products { get; set; } = new();
    public bool ValidateStock { get; set; } = true;
    public bool NotifySuppliers { get; set; } = false;
    
    [FromHeader("X-Batch-Id")]
    public string? BatchId { get; set; }
}

[OpenApiSummary("Bulk update products with transaction support")]
[OpenApiResponse(200, typeof(Response<BulkUpdateProductsResponse>), "Bulk update completed")]
[OpenApiResponse(207, typeof(Response<BulkUpdateProductsResponse>), "Partial success with errors")]
[OpenApiResponse(409, Description = "Concurrency conflict detected")]
public class BulkUpdateProductsEndpoint(IMediator mediator)
    : SingleEndpointBase<BulkUpdateProductsRequest, BulkUpdateProductsResponse>(mediator)
{
    [HttpPatch("api/products/bulk")]
    public override async Task<Response<BulkUpdateProductsResponse>> HandleAsync(
        BulkUpdateProductsRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
}
```

#### Real-Time WebSocket Integration

```csharp
public class GetLiveNotificationsRequest : IQueryRequest<NotificationStreamDto>
{
    [FromRoute]
    public string UserId { get; set; } = string.Empty;
    
    public List<NotificationType>? Types { get; set; }
    public bool IncludeRead { get; set; } = false;
    
    [FromHeader("X-Connection-Id")]
    public string? ConnectionId { get; set; }
}

[OpenApiSummary("Get live notifications stream")]
[OpenApiResponse(200, typeof(Response<NotificationStreamDto>), "Stream established")]
[Authorize]
public class GetLiveNotificationsEndpoint(IMediator mediator)
    : SingleEndpointBase<GetLiveNotificationsRequest, NotificationStreamDto>(mediator)
{
    [HttpGet("api/users/{userId}/notifications/live")]
    public override async Task<Response<NotificationStreamDto>> HandleAsync(
        GetLiveNotificationsRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
}
```

## Production-Ready Features

### Error Handling and Validation

The framework provides comprehensive error handling out of the box:

```csharp
// Automatic validation error responses
public class CreateOrderRequest : ICommand<CreateOrderResponse>
{
    [Required, Range(1, int.MaxValue)]
    public int CustomerId { get; set; }
    
    [Required, MinLength(1)]
    public List<OrderItemDto> Items { get; set; } = new();
    
    [Required, RegularExpression(@"^[A-Z]{3}$")]
    public string CurrencyCode { get; set; } = "USD";
    
    [EmailAddress]
    public string? NotificationEmail { get; set; }
}

// Framework automatically returns 400 with detailed validation errors
// No additional validation code needed in the endpoint
```

### Performance Optimization with Caching

```csharp
public class GetPopularProductsRequest : IQueryCollectionRequest<ProductDto>
{
    public int CategoryId { get; set; }
    public int Count { get; set; } = 10;
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);
}

[OpenApiSummary("Get popular products with intelligent caching")]
[ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "categoryId", "count" })]
public class GetPopularProductsEndpoint(IMediator mediator)
    : CollectionEndpointBase<GetPopularProductsRequest, ProductDto>(mediator)
{
    [HttpGet("api/categories/{categoryId}/products/popular")]
    public override async Task<ResponseCollection<ProductDto>> HandleAsync(
        GetPopularProductsRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
}
```

### Security and Rate Limiting

```csharp
[OpenApiSummary("Send password reset email")]
[OpenApiResponse(200, Description = "Reset email sent if account exists")]
[EnableRateLimiting("auth-policy")]  // Custom rate limiting
public class SendPasswordResetEndpoint(IMediator mediator)
    : SingleEndpointBase<SendPasswordResetRequest, SendPasswordResetResponse>(mediator)
{
    [HttpPost("api/auth/reset-password")]
    public override async Task<Response<SendPasswordResetResponse>> HandleAsync(
        SendPasswordResetRequest req, CancellationToken ct)
        => await _mediator.Send(req, ct);
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

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Leverages extension methods and utilities for enhanced functionality
- **Moclawr.Shared**: Uses standardized response models and entity interfaces  
- **Moclawr.Host**: Perfect companion for complete API solutions with global exception handling
- **Moclawr.Services.Autofac**: Compatible with Autofac dependency injection container
- **Moclawr.EfCore**: Works with EF Core repositories in CQRS handlers
- **Moclawr.MongoDb**: Supports MongoDB repositories in command/query handlers
- **Moclawr.Services.Caching**: Integrates with caching strategies in endpoint handlers

## Requirements

- .NET 9.0 or higher
- MediatR 12.2.0 or higher  
- Microsoft.AspNetCore.OpenApi 9.0.0 or higher
- Swashbuckle.AspNetCore.SwaggerGen 8.1.2 or higher
- Swashbuckle.AspNetCore.SwaggerUI 8.1.2 or higher

## License

This package is licensed under the MIT License.