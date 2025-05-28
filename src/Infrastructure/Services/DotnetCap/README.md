# Moclawr.DotNetCore.CAP

[![NuGet](https://img.shields.io/nuget/v/Moclawr.DotNetCore.CAP.svg)](https://www.nuget.org/packages/Moclawr.DotNetCore.CAP/)

## Overview

Moclawr.DotNetCore.CAP provides an enhanced implementation of the DotNetCore.CAP library, simplifying the integration of event-based microservices and distributed transaction processing across multiple message queuing platforms and databases. It offers a consistent configuration interface for various message brokers including RabbitMQ, Kafka, Redis Streams, and Azure Service Bus.

## Features

- **Unified Configuration**: Standardized configuration approach for all supported message brokers
- **Multiple Message Broker Support**:
  - RabbitMQ
  - Kafka
  - Redis Streams
  - Azure Service Bus
  - Pulsar
- **Database Storage Options**:
  - SQL Server
  - MySQL
  - PostgreSQL
  - MongoDB
  - In-Memory (for development/testing)
- **Built-in Dashboard**: Monitor message publication and consumption through a web interface
- **Distributed Transaction Support**: Ensures data consistency across services
- **Auto-configuration**: Simplified setup with minimal code

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.DotNetCore.CAP
```

## Usage

### Basic Configuration

In your `Program.cs` or `Startup.cs`, configure the DotnetCap services:

```csharp
using DotnetCap;

// Register DotnetCap services
builder.Services.AddDotnetCap(builder.Configuration);
```

### Configuration in appsettings.json

```json
{
  "DotnetCap": {
    "DbProvider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=capdb;Username=postgres;Password=password",
    "UseDashboard": true,
    "DashboardPath": "/cap-dashboard",
    "EnabledDashboardAuth": false,
    "DashboardAuthUser": "admin",
    "DashboardAuthPassword": "admin"
  }
}
```

### Configuring RabbitMQ

```csharp
// Register RabbitMQ specific configuration
builder.Services.AddRabbitMq(builder.Configuration);
```

Configuration in appsettings.json:

```json
{
  "DotnetCap": {
    "RabbitMQOptions": {
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest",
      "VirtualHost": "/",
      "ExchangeName": "cap.exchange",
      "PublishConfirms": true,
      "QueueArguments": {
        "QueueMode": "lazy",
        "MessageTTL": 3600000,
        "QueueType": "classic"
      },
      "QueueOptions": {
        "Durable": true,
        "Exclusive": false,
        "AutoDelete": false
      }
    }
  }
}
```

### Configuring Kafka

```csharp
// Register Kafka specific configuration
builder.Services.AddKafka(builder.Configuration);
```

Configuration in appsettings.json:

```json
{
  "DotnetCap": {
    "KafkaOptions": {
      "BootstrapServers": ["localhost:9092"],
      "ConnectionPoolSize": 10,
      "GroupId": "cap-consumer-group",
      "ClientId": "cap-client",
      "SecurityProtocol": "PLAINTEXT",
      "SaslMechanism": "PLAIN",
      "SaslUsername": "",
      "SaslPassword": "",
      "MainConfig": {
        "auto.offset.reset": "earliest",
        "enable.auto.commit": "false"
      }
    }
  }
}
```

### Publishing Messages

```csharp
using DotNetCore.CAP;

public class OrderService
{
    private readonly ICapPublisher _capPublisher;

    public OrderService(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    public async Task CreateOrderAsync(Order order)
    {
        // Business logic...
        
        // Publish the order created event
        await _capPublisher.PublishAsync("order.created", order);
    }
}
```

### Subscribing to Messages

```csharp
using DotNetCore.CAP;

public class OrderConsumer : ICapSubscribe
{
    [CapSubscribe("order.created")]
    public async Task HandleOrderCreatedAsync(Order order)
    {
        // Process the order...
        await Task.CompletedTask;
    }
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Provides configuration models and utilities used by DotnetCap
- **Moclawr.Shared**: Uses standardized response models for message handling
- **Moclawr.Host**: Perfect companion for building complete API solutions with event-driven architecture
- **Moclawr.EfCore**: Integrates with Entity Framework Core for transactional messaging and outbox patterns
- **Moclawr.MongoDb**: Supports MongoDB integration for document-based event storage
- **Moclawr.MinimalAPI**: Use with endpoint handlers for publishing events in API operations
- **Moclawr.Services.External**: Trigger external notifications through event-driven messaging

## Requirements

- .NET 9.0 or higher
- DotNetCore.CAP 8.3.4 or higher
- One or more of the supported message brokers (RabbitMQ, Kafka, Redis, etc.)

## License

This package is licensed under the MIT License.
