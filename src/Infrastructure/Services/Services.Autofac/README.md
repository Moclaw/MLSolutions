# Moclawr.Services.Autofac

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Services.Autofac.svg)](https://www.nuget.org/packages/Moclawr.Services.Autofac/)

## Overview

Moclawr.Services.Autofac provides seamless integration with Autofac dependency injection container for .NET applications. It offers convention-based registration, attribute-based service registration, and modular configuration to enhance your application's dependency injection capabilities.

## Features

- **Simple Registration Methods**: Easy-to-use extension methods following the same pattern as other Moclawr services
- **Attribute-based Registration**: Register services with custom attributes for different lifetimes (Transient, Scoped, Singleton)
- **Assembly Scanning**: Automatically register services based on conventions from specified assemblies
- **Module-based Organization**: Group related registrations into Autofac modules
- **Built-in Modules**:
  - `ServiceModule`: Registers application services by convention and attributes
  - `GenericModule`: Handles open generic type registrations
  - `ControllerModule`: Registers MVC controllers for ASP.NET Core applications
- **ASP.NET Core Integration**: Extension methods for WebApplicationBuilder and IHostBuilder

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.Services.Autofac
```

## Usage

### Basic Integration with ASP.NET Core

In your `Program.cs`:

```csharp
using Services.Autofac;

var builder = WebApplication.CreateBuilder(args);

// Configure Autofac as the service provider
builder.AddAutofacServiceProvider(
    // Optional: configure container manually
    configureContainer: container => {
        // Add manual registrations
        container.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();
    },
    // Assemblies to scan for services
    typeof(Program).Assembly,
    typeof(ApplicationLayer).Assembly
);

// Add other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Alternative Registration Methods

```csharp
// For Generic Host applications
var hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder.AddAutofacServiceProvider(
    configureContainer: container => {
        // Configure container
    },
    typeof(Program).Assembly
);

// For existing service collections
var services = new ServiceCollection();
var serviceProvider = services.AddAutofacServices(
    configureContainer: container => {
        // Configure container
    },
    typeof(Program).Assembly
);
```

### Attribute-based Service Registration

Use attributes to mark services with their intended lifetimes:

```csharp
using Services.Autofac.Attributes;

// Mark interfaces with ServiceContract attribute (optional but recommended)
[ServiceContract]
public interface IUserService
{
    Task<User> GetUserByIdAsync(int userId);
    Task<bool> CreateUserAsync(User user);
}

// Mark implementation classes with lifetime attributes
[ScopedService]
public class UserService : IUserService
{
    // Implementation
}

[TransientService]
public class EmailService : IEmailService
{
    // Implementation
}

[SingletonService]
public class ConfigurationService : IConfigurationService
{
    // Implementation
}
```

### Manual Container Configuration

```csharp
builder.AddAutofacServiceProvider(container => {
    // Register individual services
    container.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();
    
    // Register generic types
    container.AddGenericServices(typeof(Program).Assembly);
    
    // Register controllers
    container.AddControllers(typeof(Program).Assembly);
    
    // Register assemblies with specific options
    container.AddServiceAssemblies(
        registerByNamingConvention: true,
        autoRegisterConcreteTypes: false,
        typeof(Program).Assembly
    );
});
```

### Convention-based Registration

Services ending with "Service", "Repository", or "Manager" are automatically registered:

```csharp
// These will be automatically registered
public interface IUserRepository { }
public class UserRepository : IUserRepository { } // Automatically registered as IUserRepository

public interface IEmailService { }
public class EmailService : IEmailService { } // Automatically registered as IEmailService

public interface IDataManager { }
public class DataManager : IDataManager { } // Automatically registered as IDataManager
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Provides core utilities and extensions for enhanced functionality
- **Moclawr.Shared**: Uses dependency injection for shared services and interfaces
- **Moclawr.Host**: Can be used together for building complete API solutions with advanced DI patterns
- **Moclawr.MinimalAPI**: Perfect for dependency injection in Minimal API applications with MediatR
- **Moclawr.EfCore**: Registers repository and context services automatically
- **Moclawr.MongoDb**: Supports MongoDB context and repository registration
- **Moclawr.Services.Caching**: Integrates caching services with Autofac container

## Requirements

- .NET 9.0 or higher
- Autofac 8.3.0 or higher
- Autofac.Extensions.DependencyInjection 10.0.0 or higher

## License

This package is licensed under the MIT License.
