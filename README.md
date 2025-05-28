# Moclaw Overview

This directory contains the core source code for the Moclaw project, organized into several main modules:

## NuGet Packages & Installation

### Available NuGet Packages

#### Core Libraries
- **Moclawr.Core** (v2.1.0) - Essential utility extensions and base functionality
- **Moclawr.Shared** (v2.0.1) - Fundamental interfaces, response models, and utilities
- **Moclawr.Domain** (v2.1.0) - Domain modeling components and base entity types
- **Moclawr.Host** (v2.1.0) - Infrastructure for robust ASP.NET Core applications

#### Data Access (ORM)
- **Moclawr.EfCore** (v2.0.1) - Entity Framework Core utilities and patterns
- **Moclawr.MongoDb** (v2.1.0) - MongoDB integration and repository patterns

#### Services
- **Moclawr.MinimalAPI** (v2.1.0) - Class-based Minimal API with MediatR and SwaggerUI
- **Moclawr.Services.Caching** (v2.1.0) - Flexible caching with Redis and in-memory support
- **Moclawr.Services.External** (v2.1.0) - External service integrations (Email, SMS)
- **Moclawr.Services.Autofac** (v2.1.0) - Autofac dependency injection integration
- **Moclawr.DotNetCore.CAP** (v2.1.0) - Event-driven messaging with CAP framework
- **Moclawr.Services.AWS.S3** (v1.0.0) - AWS S3 service integration

### How to Install

You can install the required NuGet packages using the .NET CLI:

```sh
```sh
# Install Core Libraries
dotnet add package Moclawr.Core
dotnet add package Moclawr.Shared
dotnet add package Moclawr.Domain
dotnet add package Moclawr.Host

# Install Data Access Libraries  
dotnet add package Moclawr.EfCore
dotnet add package Moclawr.MongoDb

# Install Service Libraries
dotnet add package Moclawr.MinimalAPI
dotnet add package Moclawr.Services.Caching
dotnet add package Moclawr.Services.External
dotnet add package Moclawr.Services.Autofac
dotnet add package Moclawr.DotNetCore.CAP
dotnet add package Moclawr.Services.AWS.S3

# Install common Microsoft packages (if needed)
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Microsoft.Extensions.Configuration.Abstractions
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add package Serilog.AspNetCore
```
```

Or, restore all packages for the solution:

```sh
dotnet restore
```

---

## Architecture Overview

The MLSolutions framework follows Clean Architecture principles and Domain-Driven Design (DDD) patterns:

### Core Layer
- **Moclawr.Core**: Foundation utilities and extension methods
- **Moclawr.Shared**: Common interfaces, exceptions, and response models
- **Moclawr.Domain**: Domain entities, specifications, and business logic primitives

### Infrastructure Layer
- **Data Access**: EfCore for relational databases, MongoDb for document storage
- **Caching**: Redis distributed cache and in-memory caching strategies
- **Messaging**: CAP framework for event-driven architecture
- **External Services**: Email, SMS, and cloud storage integrations

### Application Layer  
- **Moclawr.Host**: Application infrastructure, logging, health checks, and middleware
- **Moclawr.MinimalAPI**: Endpoint definitions with MediatR CQRS pattern

### Service Layer
- **Dependency Injection**: Autofac container integration
- **Documentation**: Enhanced OpenAPI and SwaggerUI with rich documentation

---

## Setup 
- This is a Bash/PowerShell script that automatically sets up a Clean Architecture project structure for .NET applications, following Domain-Driven Design (DDD) and optional Test-Driven Development (TDD) standards [Script Setup](https://github.com/Moclaw/ScriptKids/tree/main/src/CleanArchitecture). 

## 1. Core
- **Extensions**: Provides utility extension methods for enums, objects, queryables, reflection, and strings. These extensions simplify common operations such as enum metadata retrieval, object serialization, dynamic query ordering, and string normalization.

## 2. Infrastructure
- **Domain**: Defines base entity types and builder interfaces for entity construction and query building. Includes:
  - `BaseEntity` and `TrackEntity` for entity modeling and audit tracking.
  - Fluent builder interfaces for constructing queries and setting entity properties.
- **ORM**: Contains data access logic for both Entity Framework Core (EfCore) and MongoDB:
  - **EfCore**: Base context, repositories, and builders for relational data access, supporting advanced query building, Dapper integration, and transaction management.
  - **MongoDb**: Context and repositories for MongoDB, supporting similar repository patterns as EfCore.
- **Services**: Provides service-level utilities and configurations:
  - **MinimalAPI**: Class-based Minimal API framework with MediatR integration and enhanced SwaggerUI
  - **Caching**: Redis and in-memory cache abstraction with flexible key management and removal strategies
  - **External**: External service integrations for email and SMS functionality
  - **Autofac**: Autofac dependency injection container integration for ASP.NET Core
  - **CAP**: Event-driven messaging and distributed transactions using CAP framework
  - **AWS.S3**: Amazon S3 cloud storage service integration

## 3. Shared

- **Entities**: Base interfaces for entity modeling with typed IDs
- **Exceptions**: Custom exception types for business logic, entity conflicts, and not-found scenarios  
- **Responses**: Standardized response interfaces and records for API/service responses
- **Settings**: Default JSON serialization settings for consistent data handling
- **Utils**: Utility classes for object manipulation, paging, password hashing, and response creation

## Key Features

- **Extensible Query Building**: Fluent interfaces and builders for constructing complex queries with support for navigation properties, ordering, and filtering
- **Repository Pattern**: Unified repository interfaces for both relational (EfCore) and NoSQL (MongoDB) data sources
- **Advanced Caching**: Redis and in-memory cache support with flexible key management and removal strategies  
- **Minimal API Framework**: Class-based approach to Minimal APIs with MediatR integration and enhanced SwaggerUI
- **Global Exception Handling**: Centralized error handling with custom exception handlers and structured logging
- **Health Checks**: Database and service health monitoring with configurable endpoints
- **Event-Driven Architecture**: Message publishing and subscription using CAP framework
- **Cloud Integration**: AWS S3 support for file storage and external service integrations
- **Standardized Responses**: Consistent response types for success, error, and collection results
- **Robust Utilities**: Helpers for serialization, password security, paging, and configuration management

## Usage

- Use the extension methods in `Core/Extensions` to simplify common .NET operations
- Implement domain entities by inheriting from `BaseEntity` or `TrackEntity`  
- Use the repository interfaces in `Infrastructure/ORM` for data access abstraction
- Configure caching and module settings via `Services/Configurations`
- Handle errors and responses using types in `Shared/Exceptions` and `Shared/Responses`

---

## Quick Start Guide

### 1. Create a New Project
```powershell
# Create a new API project
dotnet new webapi -n MyApp
cd MyApp

# Add Moclawr packages
dotnet add package Moclawr.Host
dotnet add package Moclawr.MinimalAPI
dotnet add package Moclawr.EfCore
```

### 2. Configure Services (Program.cs)
```csharp
using Host;
using MinimalAPI;

var builder = WebApplication.CreateBuilder(args);

// Add Moclawr services
builder.Services.AddMinimalApiWithSwaggerUI("My API", "v1");
builder.Services.AddGlobalExceptionHandling("MyApp");
builder.AddSerilog(builder.Configuration, "MyApp");

var app = builder.Build();

// Configure pipeline
app.UseGlobalExceptionHandling();
app.UseMinimalApiDocs();
app.MapMinimalEndpoints(typeof(Program).Assembly);

app.Run();
```

### 3. Create Your First Endpoint
```csharp
[OpenApiSummary("Get all items")]
public class GetItemsEndpoint(IMediator mediator) 
    : CollectionEndpointBase<GetItemsQuery, ItemDto>(mediator)
{
    [HttpGet("/api/items")]
    public override async Task<ResponseCollection<ItemDto>> HandleAsync(
        GetItemsQuery request, CancellationToken ct)
    {
        return await _mediator.Send(request, ct);
    }
}
```

---

## Sample Project

The repository includes a comprehensive sample implementation demonstrating the framework capabilities:

**Todo List CRUD Application** (`/sample/`)
- Complete Clean Architecture implementation
- Demonstrates MinimalAPI with MediatR CQRS pattern  
- Entity Framework Core integration with SQLite
- Comprehensive API documentation with SwaggerUI
- Health checks and structured logging
- Global exception handling

### Sample Features
- **CRUD Operations**: Create, Read, Update, Delete todo items
- **Advanced Filtering**: Search, pagination, and sorting capabilities
- **Rich Documentation**: Enhanced OpenAPI with interactive SwaggerUI
- **Clean Architecture**: Proper separation of concerns across layers
- **Testing Ready**: Includes test project structure

---

This structure enables scalable, maintainable, and testable application development for .NET projects with support for both relational and NoSQL databases, advanced caching, and robust domain modeling.
