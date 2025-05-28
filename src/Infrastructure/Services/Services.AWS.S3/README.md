# Services.AWS.S3

AWS S3 service integration for MLSolutions projects with LocalStack support for local development.

## Overview

This package provides a simple and consistent way to interact with AWS S3 storage in .NET applications. It includes features for uploading, downloading, and managing files in S3 buckets, with full LocalStack support for local development and testing.

## Features

- File Upload/Download
- File Deletion
- Pre-signed URL Generation
- Object Listing with Recursive Support
- Object Copy between Paths/Buckets
- File Existence Check
- Bucket Creation (useful for LocalStack)
- Configurable AWS Credentials (Direct or from AWS Credentials file)
- **LocalStack Integration** for local development

## Installation

The package is available as a NuGet package. Add it to your project using:

```shell
dotnet add package Services.AWS.S3
```

## Configuration

### Production Configuration

Add the following configuration in your `appsettings.json`:

```json
{
  "AWS": {
    "S3": {
      "AccessKey": "your-access-key",
      "SecretKey": "your-secret-key",
      "BucketName": "your-bucket-name",
      "Region": "us-east-1",
      "UseCredentialsFile": false,
      "CredentialsFilePath": "",
      "UseLocalStack": false
    }
  }
}
```

### LocalStack Configuration (Development)

For local development with LocalStack, use this configuration in your `appsettings.Development.json`:

```json
{
  "AWS": {
    "S3": {
      "AccessKey": "test",
      "SecretKey": "test",
      "BucketName": "test-bucket",
      "Region": "us-east-1",
      "UseLocalStack": true,
      "LocalStackServiceUrl": "http://localhost:4566",
      "ForcePathStyle": true
    }
  }
}
```

## LocalStack Setup

### Quick Start (No Docker)

For development without Docker, you can disable S3 features or use a mock implementation:

```json
{
  "AWS": {
    "S3": {
      "AccessKey": "",
      "SecretKey": "",
      "BucketName": "",
      "Region": "us-east-1",
      "UseLocalStack": false,
      "Disabled": true
    }
  }
}
```

### Using Docker Compose

1. Start LocalStack using the provided Docker Compose file:

```bash
docker-compose -f docker-compose.localstack.yml up -d
```

2. The LocalStack S3 service will be available at `http://localhost:4566`

### Standalone Docker Commands

#### Basic Setup
```bash
docker run --rm -d \
  --name localstack-s3 \
  -p 4566:4566 \
  -e SERVICES=s3 \
  localstack/localstack:latest
```

#### Advanced Setup with Persistence
```bash
docker run --rm -d \
  --name localstack-s3 \
  -p 4566:4566 \
  -e SERVICES=s3 \
  -e DEBUG=1 \
  -e AWS_DEFAULT_REGION=us-east-1 \
  -e AWS_ACCESS_KEY_ID=test \
  -e AWS_SECRET_ACCESS_KEY=test \
  -v localstack-data:/tmp/localstack \
  localstack/localstack:latest
```

#### Stop LocalStack
```bash
docker stop localstack-s3
```

#### View LocalStack Logs
```bash
docker logs localstack-s3
```

### Standalone LocalStack Setup

If you prefer to run LocalStack separately:

1. Install LocalStack CLI:
```bash
pip install localstack
```

2. Start LocalStack:
```bash
localstack start -d
```

3. Create S3 bucket manually (optional):
```bash
aws --endpoint-url=http://localhost:4566 s3 mb s3://test-bucket
```

## Usage

### Service Registration

Register the S3 service in your `Program.cs` or `Startup.cs`:

```csharp
using Services.AWS.S3;

// Simple registration with default configuration section "AWS:S3"
services.AddS3Services(Configuration);

// Or with custom configuration section
services.AddS3Services(Configuration, "MyCustom:S3Section");

// Or with inline configuration
services.AddS3Services(options =>
{
    options.AccessKey = "test";
    options.SecretKey = "test";
    options.BucketName = "my-bucket";
    options.Region = "us-east-1";
    options.UseLocalStack = true;
});
```

### Legacy Registration (Still Supported)

```csharp
using Services.AWS.S3.Extensions;

// Legacy method
services.AddS3Service(Configuration);
```

### Basic Usage

```csharp
public class FileService
{
    private readonly IS3Service _s3Service;

    public FileService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    // Ensure bucket exists (useful for LocalStack)
    public async Task InitializeAsync()
    {
        await _s3Service.EnsureBucketExistsAsync();
    }

    public async Task UploadFileAsync(string key, Stream content, string contentType)
    {
        await _s3Service.UploadFileAsync(key, content, contentType);
    }

    public async Task<Stream> DownloadFileAsync(string key)
    {
        var response = await _s3Service.DownloadFileAsync(key);
        return response.ResponseStream;
    }

    public async Task<string> GetFileUrlAsync(string key)
    {
        return await _s3Service.GetPreSignedUrlAsync(key, expiryMinutes: 60);
    }
}
```

### Advanced Usage Examples

```csharp
// List all objects in a directory
var objects = await _s3Service.ListObjectsAsync("documents/", recursive: true);

// Copy object to another location
await _s3Service.CopyObjectAsync("source/file.pdf", "backup/file.pdf");

// Check if file exists
bool exists = await _s3Service.FileExistsAsync("documents/important.pdf");

// Delete file
await _s3Service.DeleteFileAsync("old/file.pdf");
```

## Development Workflow

1. **Start LocalStack**: Run `docker-compose -f docker-compose.localstack.yml up -d`
2. **Configure Development Settings**: Use LocalStack configuration in `appsettings.Development.json`
3. **Initialize Bucket**: Call `EnsureBucketExistsAsync()` to create the bucket in LocalStack
4. **Develop and Test**: Your S3 operations will work against LocalStack
5. **Deploy**: Switch to production configuration for deployment

## Integration with Other MLSolutions Packages

This package works seamlessly with other packages in the MLSolutions ecosystem:

- **Moclawr.Core**: Leverages configuration models and utility extensions
- **Moclawr.Shared**: Uses standardized response models for consistent error handling
- **Moclawr.Host**: Perfect companion for building complete API solutions with cloud storage capabilities
- **Moclawr.MinimalAPI**: Integrates with endpoint handlers for file upload/download operations
- **Moclawr.Services.Caching**: Cache S3 metadata and file information for improved performance
- **Moclawr.Services.External**: Use together for comprehensive cloud service integrations

## Requirements

- .NET 9.0 or higher
- AWSSDK.S3 4.0.0 or higher
- Docker (for LocalStack development)

## License

This project is licensed under the MIT License - see the LICENSE file for details.
