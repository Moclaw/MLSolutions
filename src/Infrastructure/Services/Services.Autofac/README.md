# Moclawr.Services.Autofac

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Services.Autofac.svg)](https://www.nuget.org/packages/Moclawr.Services.Autofac/)

## Overview

Moclawr.Services.Autofac provides seamless integration with Autofac dependency injection container for .NET applications. It offers convention-based registration, attribute-based service registration, and modular configuration to enhance your application's dependency injection capabilities.

## Features

- **Attribute-based Registration**: Register services with custom attributes for different lifetimes (Transient, Scoped, Singleton)
- **Assembly Scanning**: Automatically register services based on conventions from specified assemblies
- **Module-based Organization**: Group related registrations into Autofac modules
- **Built-in Modules**:
  - `ServiceModule`: Registers application services
  - `RepositoryModule`: Registers repositories
  - `ControllerModule`: Registers controllers for ASP.NET Core applications
  - `AttributeModule`: Handles attribute-based registration
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
using Services.Autofac.Extensions;

// Configure the host to use Autofac
var builder = WebApplication.CreateBuilder(args);

// Use Autofac as the service provider
builder.UseAutofacServiceProvider(
    // Optional: configure container manually
    configureContainer: builder => {
        // Add manual registrations
        builder.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();
    },
    // Assemblies to scan for services
    typeof(Program).Assembly,
    typeof(ApplicationLayer).Assembly
);

// Add services to the container
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

### Attribute-based Service Registration

Use attributes to mark services with their intended lifetimes:

```csharp
using Services.Autofac.Attributes;

// Mark interfaces with ServiceContract attribute
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

### Custom Modules

Create custom Autofac modules for specific registration logic:

```csharp
using Autofac;
using Services.Autofac.Modules;

public class DatabaseModule : BaseModule
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register database-related services
        builder.RegisterType<DbContext>()
            .AsSelf()
            .InstancePerLifetimeScope();
            
        builder.RegisterGeneric(typeof(GenericRepository<>))
            .As(typeof(IGenericRepository<>))
            .InstancePerLifetimeScope();
            
        base.Load(builder);
    }
}

// Use the custom module
builder.UseAutofacServiceProvider(
    configureContainer: builder => {
        builder.RegisterModule<DatabaseModule>();
    },
    typeof(Program).Assembly
);
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Provides core utilities and extensions
- **Moclawr.Host**: Can be used together for building complete API solutions
- **Moclawr.MinimalAPI**: Works with Autofac for dependency injection in Minimal API applications

## Requirements

- .NET 9.0 or higher
- Autofac 8.3.0 or higher
- Autofac.Extensions.DependencyInjection 10.0.0 or higher

## License

This package is licensed under the MIT License.
