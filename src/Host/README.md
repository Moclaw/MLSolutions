# Moclawr.Host

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Host.svg)](https://www.nuget.org/packages/Moclawr.Host/)

## Overview

Moclawr.Host provides essential infrastructure for building robust and maintainable ASP.NET Core applications. It includes global exception handling, health checks, logging with Serilog, and various middleware components to streamline application setup and improve error handling and monitoring capabilities.

## Features

- **Global Exception Handling**: Centralized error handling with custom exception handlers
- **Health Checks**: Database and service health monitoring
- **Structured Logging**: Pre-configured Serilog integration with multiple sinks (Console, File, Elasticsearch)
- **APM Integration**: Elastic APM for application performance monitoring
- **CORS Configuration**: Simplified cross-origin resource sharing setup
- **Custom Middleware**: Pre-built middleware for common application needs
- **Security Headers**: Configurable security headers for web applications
- **Authorization Attributes**: Custom authorization attributes for securing endpoints

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.Host
```

## Usage

### Setting Up Global Exception Handling

In your `Program.cs`:

```csharp
using Host;

// Add global exception handling
builder.Services.AddGlobalExceptionHandling("MyApplication");

// Configure the application
var app = builder.Build();

// Use global exception handling middleware
app.UseGlobalExceptionHandling();
```

### Configuring Serilog

```csharp
// Add Serilog for structured logging
builder.AddSerilog(builder.Configuration, "MyApplication");
```

Configuration in appsettings.json:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log-.json",
          "rollingInterval": "Hour",
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Adding Health Checks

```csharp
// Add health checks for database and external services
builder.Services.AddHealthChecks(builder.Configuration);

// In the app configuration section
app.MapHealthChecks();
```

### Setting Up CORS

```csharp
// Add CORS services
builder.Services.AddCorsServices(builder.Configuration);

// In the app configuration section
app.UseCors("DefaultPolicy");
```

Configuration in appsettings.json:

```json
{
  "Cors": {
    "AllowedOrigins": ["https://example.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Content-Type", "Authorization"],
    "AllowCredentials": true
  }
}
```

### Adding Database Health Checks

```csharp
// Add database context health check
builder.Services.AddDbContextHealthCheck<ApplicationDbContext>("Database");
```

### Using Elastic APM

```csharp
// Configure Elastic APM
builder.Services.AddElasticApm(builder.Configuration);

// In the app configuration section
app.UseAllElasticApm(builder.Configuration);
```

Configuration in appsettings.json:

```json
{
  "ElasticApm": {
    "ServerUrl": "http://localhost:8200",
    "ServiceName": "MyApplication",
    "Environment": "Development"
  }
}
```

### Using Custom Exception Handlers

You can create custom exception handlers by implementing the `IExceptionHandler` interface:

```csharp
[HandlerFor(typeof(MyCustomException))]
public class MyCustomExceptionHandler : IExceptionHandler
{
    public async Task HandleException(HttpContext context, Exception exception, ILogger logger)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var response = new 
        {
            Status = 400,
            Message = "A custom error occurred",
            Details = exception.Message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Provides core utilities and extensions
- **Moclawr.Shared**: Contains shared models and settings
- **Moclawr.EfCore**: Integrates with Entity Framework Core for database health checks

## Requirements

- .NET 9.0 or higher
- Serilog.AspNetCore 9.0.0 or higher
- AspNetCore.HealthChecks.UI.Client 9.0.0 or higher
- Microsoft.EntityFrameworkCore 9.0.4 or higher (for database health checks)

## License

This package is licensed under the MIT License.
