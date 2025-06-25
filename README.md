# MLSolutions Framework

A comprehensive .NET framework for building scalable, maintainable enterprise applications using Clean Architecture principles and Domain-Driven Design (DDD) patterns.

## 🚀 Key Features

- **Clean Architecture**: Proper separation of concerns with clear layer boundaries
- **CQRS & MediatR**: Command Query Responsibility Segregation with MediatR integration
- **MinimalAPI Framework**: Class-based Minimal APIs with automatic endpoint discovery
- **Advanced Repository Pattern**: Unified interfaces for EF Core and MongoDB
- **Event-Driven Architecture**: Built-in messaging with CAP framework
- **Comprehensive Documentation**: Enhanced OpenAPI/SwaggerUI with versioning support
- **Global Exception Handling**: Centralized error management with structured logging
- **Flexible Caching**: Redis and in-memory caching with intelligent invalidation
- **Cloud Integration**: AWS S3 and external service connectors
- **Health Monitoring**: Advanced health checks for databases and services

## 📦 NuGet Packages

### Core Libraries
| Package | Version | Description |
|---------|---------|-------------|
| **Moclawr.Core** | v2.1.0 | Essential utilities, extensions, and base functionality |
| **Moclawr.Shared** | v2.0.1 | Common interfaces, response models, and utilities |
| **Moclawr.Domain** | v2.1.0 | Domain modeling, base entities, and builder patterns |
| **Moclawr.Host** | v2.1.0 | Application infrastructure, logging, and middleware |

### Data Access (ORM)
| Package | Version | Description |
|---------|---------|-------------|
| **Moclawr.EfCore** | v2.0.1 | Entity Framework Core with repository patterns and advanced querying |
| **Moclawr.MongoDb** | v2.1.0 | MongoDB integration with EF Core-like API and repository patterns |

### Services & Infrastructure
| Package | Version | Description |
|---------|---------|-------------|
| **Moclawr.MinimalAPI** | v2.1.0 | Class-based Minimal API with MediatR, versioning, and enhanced SwaggerUI |
| **Moclawr.Services.Caching** | v2.1.0 | Redis and in-memory caching with flexible invalidation strategies |
| **Moclawr.Services.External** | v2.1.0 | External service integrations (Email, SMS, notifications) |
| **Moclawr.Services.Autofac** | v2.1.0 | Autofac dependency injection container integration |
| **Moclawr.DotNetCore.CAP** | v2.1.0 | Event-driven messaging and distributed transactions |
| **Moclawr.Services.AWS.S3** | v1.0.0 | AWS S3 cloud storage integration with LocalStack support |
| **Moclawr.Services.AWS.SecretsManager** | v2.0.0 | AWS Secrets Manager integration for secure secret management |

## 🛠 Installation

### Quick Start - All Packages
```bash
# Install all core packages
dotnet add package Moclawr.Core
dotnet add package Moclawr.Shared
dotnet add package Moclawr.Domain
dotnet add package Moclawr.Host

# Install data access
dotnet add package Moclawr.EfCore
# OR
dotnet add package Moclawr.MongoDb

# Install API framework
dotnet add package Moclawr.MinimalAPI

# Install services (optional)
dotnet add package Moclawr.Services.Caching
dotnet add package Moclawr.Services.External
dotnet add package Moclawr.Services.Autofac
dotnet add package Moclawr.DotNetCore.CAP
dotnet add package Moclawr.Services.AWS.S3
```

### Targeted Installation
```bash
# For API projects
dotnet add package Moclawr.Host
dotnet add package Moclawr.MinimalAPI
dotnet add package Moclawr.EfCore

# For domain projects
dotnet add package Moclawr.Core
dotnet add package Moclawr.Domain

# For infrastructure projects
dotnet add package Moclawr.Services.Caching
dotnet add package Moclawr.Services.External
```

## 🏗 Architecture Overview

```
┌─────────────────────────────────────────────────┐
│                 Presentation                    │
│  ┌─────────────┐  ┌──────────────────────────┐  │
│  │ MinimalAPI  │  │    SwaggerUI/OpenAPI     │  │
│  │ Endpoints   │  │    Documentation         │  │
│  └─────────────┘  └──────────────────────────┘  │
└─────────────────────────────────────────────────┘
                         │
┌─────────────────────────────────────────────────┐
│                 Application                     │
│  ┌─────────────┐  ┌──────────────────────────┐  │
│  │   MediatR   │  │      CQRS Handlers       │  │
│  │   Queries   │  │      Commands/Queries    │  │
│  └─────────────┘  └──────────────────────────┘  │
└─────────────────────────────────────────────────┘
                         │
┌─────────────────────────────────────────────────┐
│                   Domain                        │
│  ┌─────────────┐  ┌──────────────────────────┐  │
│  │  Entities   │  │    Business Logic        │  │
│  │ Aggregates  │  │    Domain Services       │  │
│  └─────────────┘  └──────────────────────────┘  │
└─────────────────────────────────────────────────┘
                         │
┌─────────────────────────────────────────────────┐
│                Infrastructure                   │
│  ┌─────────────┐  ┌──────────────────────────┐  │
│  │ Repositories│  │    External Services     │  │
│  │   EF Core   │  │   Caching, Messaging     │  │
│  │   MongoDB   │  │     AWS S3, Email        │  │
│  └─────────────┘  └──────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

## 🚀 Quick Start Guide

### 1. Create a New Project
```bash
# Create API project
dotnet new webapi -n MyApp
cd MyApp

# Add essential packages
dotnet add package Moclawr.Host
dotnet add package Moclawr.MinimalAPI
dotnet add package Moclawr.EfCore
```

### 2. Configure Services (Program.cs)
```csharp
using Host;
using MinimalAPI;
using EfCore;

var builder = WebApplication.CreateBuilder(args);

// Configure application infrastructure
builder.AddSerilog(builder.Configuration, "MyApp");
builder.Services.AddGlobalExceptionHandling("MyApp");

// Add MinimalAPI with comprehensive documentation
builder.Services.AddMinimalApiWithSwaggerUI(
    title: "My API",
    version: "v1",
    description: "Comprehensive API built with MLSolutions framework"
);

// Add data access
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMinimalApiDocs();
}

app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();

// Auto-discover and map all endpoints
app.MapMinimalEndpoints(typeof(Program).Assembly);

app.Run();
```

### 3. Create Your First Endpoint
```csharp
using MinimalAPI.Endpoints;
using MinimalAPI.Attributes;
using MediatR;

[OpenApiSummary("Get all users", "Retrieves a paginated list of users")]
[ApiVersion(1)]
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

### 4. Implement CQRS Handler
```csharp
using MediatR;
using EfCore.Repositories;

public record GetUsersQuery(
    int Page = 1, 
    int PageSize = 10, 
    string? SearchTerm = null
) : IRequest<ResponseCollection<UserDto>>;

public class GetUsersHandler(IQueryRepository<User, int> queryRepository) 
    : IRequestHandler<GetUsersQuery, ResponseCollection<UserDto>>
{
    public async Task<ResponseCollection<UserDto>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await queryRepository.GetPagedListAsync<UserDto>(
            request.Page,
            request.PageSize,
            filter: u => string.IsNullOrEmpty(request.SearchTerm) || 
                        u.Name.Contains(request.SearchTerm),
            orderBy: q => q.OrderBy(u => u.Name),
            cancellationToken: cancellationToken
        );

        return ResponseHelper.CreateSuccessCollection(users);
    }
}
```

## 🎯 Key Framework Features

### MinimalAPI Framework
- **Class-based Endpoints**: Object-oriented approach to Minimal APIs
- **Automatic Discovery**: Auto-registration of endpoint classes
- **MediatR Integration**: Built-in CQRS pattern support
- **Advanced Versioning**: Multiple versioning strategies (URL, header, query, media type)
- **Enhanced Documentation**: Rich OpenAPI/SwaggerUI with automatic schema generation
- **Request Binding**: Intelligent parameter binding from various sources

### Repository Pattern
- **Unified Interface**: Common repository pattern for EF Core and MongoDB
- **CQRS Separation**: Separate command and query repositories
- **Advanced Querying**: Fluent query builders with projections
- **Transaction Support**: Built-in transaction management
- **Pagination**: Seamless pagination with metadata

### Global Infrastructure
- **Exception Handling**: Centralized error management with custom handlers
- **Structured Logging**: Serilog integration with multiple sinks
- **Health Checks**: Comprehensive health monitoring
- **CORS Configuration**: Flexible cross-origin resource sharing
- **Security Headers**: Configurable security headers

### Event-Driven Architecture
- **Message Publishing**: Event publishing with CAP framework
- **Multiple Brokers**: Support for RabbitMQ, Kafka, Redis, Azure Service Bus
- **Distributed Transactions**: Ensuring data consistency across services
- **Event Sourcing**: Built-in support for event sourcing patterns

## 📋 Sample Implementation

The framework includes a comprehensive **Todo List CRUD Application** demonstrating:

### Features Demonstrated
- **Complete CRUD Operations**: Create, read, update, delete todo items
- **Advanced Filtering**: Search, pagination, sorting, and filtering
- **API Versioning**: Multiple version strategies with comprehensive examples
- **File Storage**: AWS S3 integration with LocalStack for development
- **Secrets Management**: AWS Secrets Manager integration for secure configuration
- **Rich Documentation**: Interactive SwaggerUI with detailed endpoint documentation
- **Health Monitoring**: Database and service health checks
- **Structured Logging**: Request/response logging with Serilog

### Architecture Showcase
- **Clean Architecture**: Proper layer separation with clear boundaries
- **CQRS Pattern**: Command and query separation with MediatR
- **Repository Pattern**: Both EF Core and MongoDB implementations
- **Domain Modeling**: Rich domain entities with business logic
- **Event-Driven**: Messaging patterns for decoupled communication

### Development Setup
```bash
# Clone the repository
git clone https://github.com/your-org/MLSolutions.git
cd MLSolutions/sample

# Run with Docker (recommended)
docker-compose up -d

# Or run standalone
dotnet run --project sample.API
```

### API Examples
```bash
# Get all todos with versioning
curl "https://localhost:5001/api/v1/todos?page=1&pageSize=10"

# Create a new todo
curl -X POST "https://localhost:5001/api/v1/todos" \
  -H "Content-Type: application/json" \
  -d '{"title":"Learn MLSolutions","description":"Study the framework"}'

# Upload file to S3
curl -X POST "https://localhost:5001/api/v1/s3/upload" \
  -F "file=@document.pdf" \
  -F "folder=attachments"
```

## 🧪 Testing

### Unit Testing
```csharp
[Test]
public async Task GetUsers_ShouldReturnPagedResults()
{
    // Arrange
    var query = new GetUsersQuery(1, 10);
    
    // Act
    var result = await _handler.Handle(query, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().HaveCountLessOrEqualTo(10);
}
```

### Integration Testing
```csharp
[Test]
public async Task GetUsersEndpoint_ShouldReturn200()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/v1/users");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
```bash
# Clone the repository
git clone https://github.com/your-org/MLSolutions.git
cd MLSolutions

# Restore dependencies
dotnet restore

# Run tests
dotnet test

# Build all projects
dotnet build
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [NuGet Packages](https://www.nuget.org/profiles/Moclawr)
- [Documentation](https://docs.mlsolutions.com)
- [Sample Projects](./sample/)
- [Contributing Guidelines](CONTRIBUTING.md)
- [Security Policy](SECURITY.md)

## 📞 Support

- **Email**: mocduonglam86@gmail.com
- **Issues**: [GitHub Issues](https://github.com/your-org/MLSolutions/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/MLSolutions/discussions)

---

Built with ❤️ by the MLSolutions Team
