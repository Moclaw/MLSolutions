# Moclawr.Services.AWS.SecretsManager

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Services.AWS.SecretsManager.svg)](https://www.nuget.org/packages/Moclawr.Services.AWS.SecretsManager/)

## Overview

This package provides a comprehensive and easy-to-use interface for interacting with AWS Secrets Manager in .NET applications. It includes features for creating, retrieving, updating, and managing secrets with built-in caching, LocalStack support for local development, and strong typing for JSON secrets.

## Features

- **Secret Management**: Create, retrieve, update, and delete secrets
- **Version Control**: Support for secret versions and staging
- **Strong Typing**: Deserialize JSON secrets to strongly-typed objects
- **Caching**: Built-in memory caching for improved performance
- **Batch Operations**: Retrieve multiple secrets in a single operation
- **Secret Rotation**: Automated secret rotation support
- **Tagging**: Add and manage tags on secrets
- **LocalStack Integration** for local development and testing
- **Configurable Prefixes**: Organize secrets with prefixes
- **Comprehensive Error Handling**: Detailed exception handling and logging

## Installation

The package is available as a NuGet package. Add it to your project using:

```shell
dotnet add package Moclawr.Services.AWS.SecretsManager
```

## Configuration

### Registering Services

In your `Program.cs`:

```csharp
using Services.AWS.SecretsManager;

// Register Secrets Manager services with default configuration section
builder.Services.AddSecretsManagerServices(builder.Configuration);

// Or with custom configuration section
builder.Services.AddSecretsManagerServices(builder.Configuration, "CustomAWS:SecretsManager");
```

### Development Configuration (LocalStack)

Configure LocalStack settings in `appsettings.Development.json`:

```json
{
  "AWS": {
    "SecretsManager": {
      "AccessKey": "test",
      "SecretKey": "test",
      "Region": "us-east-1",
      "UseLocalStack": true,
      "LocalStackServiceUrl": "http://localhost:4566",
      "SecretPrefix": "dev/",
      "EnableCaching": true,
      "CacheExpirationMinutes": 15
    }
  }
}
```

### Production Configuration

Configure AWS Secrets Manager settings in `appsettings.json`:

```json
{
  "AWS": {
    "SecretsManager": {
      "AccessKey": "your-access-key",
      "SecretKey": "your-secret-key",
      "Region": "us-east-1",
      "SecretPrefix": "prod/",
      "EnableCaching": true,
      "CacheExpirationMinutes": 30
    }
  }
}
```

### Using AWS Profile (Alternative)

You can also use AWS profiles instead of access keys:

```json
{
  "AWS": {
    "SecretsManager": {
      "UseCredentialsFile": true,
      "ProfileName": "default",
      "Region": "us-east-1",
      "SecretPrefix": "myapp/",
      "EnableCaching": false
    }
  }
}
```

## Usage

### Basic Secret Operations

```csharp
using Services.AWS.SecretsManager.Interfaces;

public class DatabaseService
{
    private readonly ISecretsManagerService _secretsManager;

    public DatabaseService(ISecretsManagerService secretsManager)
    {
        _secretsManager = secretsManager;
    }

    public async Task<string> GetConnectionStringAsync()
    {
        // Get a simple string secret
        return await _secretsManager.GetSecretValueAsync("database-connection-string");
    }

    public async Task<DatabaseConfig> GetDatabaseConfigAsync()
    {
        // Get a JSON secret and deserialize to a strong type
        return await _secretsManager.GetSecretValueAsync<DatabaseConfig>("database-config");
    }
}

public class DatabaseConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

### Creating and Updating Secrets

```csharp
public class SecretManagementService
{
    private readonly ISecretsManagerService _secretsManager;

    public SecretManagementService(ISecretsManagerService secretsManager)
    {
        _secretsManager = secretsManager;
    }

    public async Task CreateApiKeySecretAsync(string serviceName, string apiKey)
    {
        var secretName = $"api-keys/{serviceName}";
        var description = $"API key for {serviceName} service";
        
        await _secretsManager.CreateSecretAsync(secretName, apiKey, description);
    }

    public async Task UpdateDatabaseConfigAsync(DatabaseConfig config)
    {
        var secretName = "database-config";
        var secretValue = JsonSerializer.Serialize(config);
        
        await _secretsManager.UpdateSecretAsync(secretName, secretValue, "Updated database configuration");
    }

    public async Task<bool> SecretExistsAsync(string secretName)
    {
        return await _secretsManager.SecretExistsAsync(secretName);
    }
}
```

### Batch Operations and Secret Management

```csharp
public class ConfigurationService
{
    private readonly ISecretsManagerService _secretsManager;

    public ConfigurationService(ISecretsManagerService secretsManager)
    {
        _secretsManager = secretsManager;
    }

    public async Task<Dictionary<string, string>> LoadAllConfigurationAsync()
    {
        var secretNames = new[]
        {
            "database-connection",
            "redis-connection",
            "jwt-secret",
            "external-api-key"
        };

        return await _secretsManager.GetMultipleSecretsAsync(secretNames);
    }

    public async Task<List<string>> ListApplicationSecretsAsync()
    {
        var filters = new List<Amazon.SecretsManager.Model.Filter>
        {
            new()
            {
                Key = Amazon.SecretsManager.FilterNameStringType.Name,
                Values = new List<string> { "myapp/*" }
            }
        };

        var secrets = await _secretsManager.ListSecretsAsync(filters: filters);
        return secrets.Select(s => s.Name).ToList();
    }
}
```

### Secret Rotation and Lifecycle Management

```csharp
public class SecretRotationService
{
    private readonly ISecretsManagerService _secretsManager;

    public SecretRotationService(ISecretsManagerService secretsManager)
    {
        _secretsManager = secretsManager;
    }

    public async Task SetupAutomaticRotationAsync(string secretName, string lambdaFunctionArn)
    {
        var rotationRules = new Amazon.SecretsManager.Model.RotationRulesType
        {
            AutomaticallyAfterDays = 30
        };

        await _secretsManager.RotateSecretAsync(secretName, lambdaFunctionArn, rotationRules);
    }

    public async Task DeleteSecretWithRecoveryAsync(string secretName)
    {
        // Delete with 7-day recovery window
        await _secretsManager.DeleteSecretAsync(secretName, recoveryWindowInDays: 7);
    }

    public async Task ForceDeleteSecretAsync(string secretName)
    {
        // Immediate deletion without recovery
        await _secretsManager.DeleteSecretAsync(secretName, forceDeleteWithoutRecovery: true);
    }
}
```

### Tagging and Metadata

```csharp
public class SecretTaggingService
{
    private readonly ISecretsManagerService _secretsManager;

    public SecretTaggingService(ISecretsManagerService secretsManager)
    {
        _secretsManager = secretsManager;
    }

    public async Task TagSecretAsync(string secretName, string environment, string application)
    {
        var tags = new List<Amazon.SecretsManager.Model.Tag>
        {
            new() { Key = "Environment", Value = environment },
            new() { Key = "Application", Value = application },
            new() { Key = "ManagedBy", Value = "MLSolutions" }
        };

        await _secretsManager.TagSecretAsync(secretName, tags);
    }

    public async Task<Amazon.SecretsManager.Model.DescribeSecretResponse> GetSecretMetadataAsync(string secretName)
    {
        return await _secretsManager.GetSecretMetadataAsync(secretName);
    }
}
```

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `AccessKey` | `""` | AWS Access Key ID |
| `SecretKey` | `""` | AWS Secret Access Key |
| `Region` | `"us-east-1"` | AWS Region |
| `UseCredentialsFile` | `false` | Use AWS credentials file instead of keys |
| `CredentialsFilePath` | `""` | Path to AWS credentials file |
| `ProfileName` | `"default"` | AWS profile name |
| `UseLocalStack` | `false` | Enable LocalStack for local development |
| `LocalStackServiceUrl` | `"http://localhost:4566"` | LocalStack service URL |
| `SecretPrefix` | `""` | Prefix for all secret names |
| `EnableCaching` | `true` | Enable in-memory caching |
| `CacheExpirationMinutes` | `15` | Cache expiration time |

## LocalStack Development Setup

For local development, you can use LocalStack to simulate AWS Secrets Manager:

### Docker Compose

```yaml
version: '3.8'
services:
  localstack:
    image: localstack/localstack:latest
    container_name: localstack
    ports:
      - "4566:4566"
    environment:
      - SERVICES=secretsmanager
      - DEBUG=1
      - DATA_DIR=/tmp/localstack/data
    volumes:
      - "./tmp/localstack:/tmp/localstack"
      - "/var/run/docker.sock:/var/run/docker.sock"
```

### Create Test Secrets

```bash
# Create a simple secret
aws --endpoint-url=http://localhost:4566 secretsmanager create-secret \
    --name "test-secret" \
    --secret-string "my-secret-value"

# Create a JSON secret
aws --endpoint-url=http://localhost:4566 secretsmanager create-secret \
    --name "database-config" \
    --secret-string '{"host":"localhost","port":5432,"database":"testdb"}'
```

## Error Handling

The service provides comprehensive error handling for common scenarios:

```csharp
try
{
    var secret = await _secretsManager.GetSecretValueAsync("non-existent-secret");
}
catch (Amazon.SecretsManager.Model.ResourceNotFoundException)
{
    // Secret doesn't exist
    Console.WriteLine("Secret not found");
}
catch (Amazon.SecretsManager.Model.InvalidRequestException ex)
{
    // Invalid request parameters
    Console.WriteLine($"Invalid request: {ex.Message}");
}
catch (Amazon.SecretsManager.Model.DecryptionFailureException)
{
    // Failed to decrypt the secret
    Console.WriteLine("Failed to decrypt secret");
}
```

## Best Practices

1. **Use Secret Prefixes**: Organize secrets with consistent prefixes (e.g., `prod/`, `dev/`, `myapp/`)
2. **Enable Caching**: Use caching for frequently accessed secrets to improve performance
3. **Strong Typing**: Use generic methods to deserialize JSON secrets to typed objects
4. **Proper Error Handling**: Always handle exceptions appropriately
5. **Secret Rotation**: Implement automatic rotation for sensitive secrets
6. **Tagging**: Use tags to organize and manage secrets effectively
7. **LocalStack Testing**: Use LocalStack for local development and testing

## Integration with Moclawr Ecosystem

This service integrates seamlessly with other Moclawr packages:

- **Moclawr.Core**: Logging and extension methods
- **Moclawr.Host**: Configuration management and health checks
- **Moclawr.Services.Caching**: External caching integration
- **Moclawr.MinimalAPI**: Configuration injection in API endpoints

## Security Considerations

- Never log secret values
- Use IAM roles and policies to restrict access
- Enable CloudTrail for audit logging
- Use encryption in transit and at rest
- Regularly rotate secrets
- Use least privilege access principles

## License

This package is licensed under the MIT License.

---

**Version**: 2.0.0  
**Compatibility**: .NET 9.0+  
**Dependencies**: AWS SDK for .NET, Microsoft.Extensions.Caching.Memory
