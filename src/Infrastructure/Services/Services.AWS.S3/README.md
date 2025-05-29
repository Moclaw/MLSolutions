# Moclawr.Services.AWS.S3

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Services.AWS.S3.svg)](https://www.nuget.org/packages/Moclawr.Services.AWS.S3/)

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
dotnet add package Moclawr.Services.AWS.S3
```

## Configuration

### Registering Services

In your `Program.cs`:

```csharp
using Services.AWS.S3;

// Register S3 services
builder.Services.AddAwsS3Services(builder.Configuration);
```

### Development Configuration (LocalStack)

Configure LocalStack settings in `appsettings.Development.json`:

```json
{
  "AWS": {
    "S3": {
      "AccessKey": "test",
      "SecretKey": "test",
      "BucketName": "my-dev-bucket",
      "Region": "us-east-1",
      "UseLocalStack": true,
      "LocalStackServiceUrl": "http://localhost:4566",
      "ForcePathStyle": true
    }
  }
}
```

### Production Configuration

Configure AWS S3 settings in `appsettings.json`:

```json
{
  "AWS": {
    "S3": {
      "AccessKey": "YOUR_ACCESS_KEY",
      "SecretKey": "YOUR_SECRET_KEY",
      "BucketName": "your-production-bucket",
      "Region": "us-west-2",
      "UseLocalStack": false
    }
  }
}
```

### Using AWS Profile (Alternative)

You can also use AWS profiles instead of access keys:

```json
{
  "AWS": {
    "S3": {
      "ProfileName": "default",
      "BucketName": "your-bucket",
      "Region": "us-west-2",
      "UseLocalStack": false
    }
  }
}
```

## Usage

### File Upload

```csharp
using Services.AWS.S3;

public class DocumentService
{
    private readonly IS3Service _s3Service;

    public DocumentService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<string> UploadDocumentAsync(IFormFile file, string folder = "documents")
    {
        using var stream = file.OpenReadStream();
        var key = $"{folder}/{Guid.NewGuid()}-{file.FileName}";
        
        var result = await _s3Service.UploadFileAsync(key, stream, file.ContentType);
        
        if (result.Success)
        {
            return result.Key;
        }
        
        throw new InvalidOperationException($"Failed to upload file: {result.ErrorMessage}");
    }

    public async Task<string> UploadFromBytesAsync(byte[] data, string fileName, string contentType = "application/octet-stream")
    {
        var key = $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}-{fileName}";
        
        using var stream = new MemoryStream(data);
        var result = await _s3Service.UploadFileAsync(key, stream, contentType);
        
        return result.Success ? result.Key : throw new InvalidOperationException(result.ErrorMessage);
    }
}
```

### File Download

```csharp
public class FileService
{
    private readonly IS3Service _s3Service;

    public FileService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<byte[]> DownloadFileAsync(string key)
    {
        var result = await _s3Service.DownloadFileAsync(key);
        
        if (result.Success && result.Data != null)
        {
            using var memoryStream = new MemoryStream();
            await result.Data.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        
        throw new FileNotFoundException($"File not found: {key}");
    }

    public async Task<Stream> DownloadFileStreamAsync(string key)
    {
        var result = await _s3Service.DownloadFileAsync(key);
        
        if (result.Success && result.Data != null)
        {
            return result.Data;
        }
        
        throw new FileNotFoundException($"File not found: {key}");
    }
}
```

### Pre-signed URLs

```csharp
public class ShareService
{
    private readonly IS3Service _s3Service;

    public ShareService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<string> GetDownloadUrlAsync(string key, int expiryMinutes = 60)
    {
        var result = await _s3Service.GeneratePresignedUrlAsync(key, TimeSpan.FromMinutes(expiryMinutes));
        
        if (result.Success)
        {
            return result.Url;
        }
        
        throw new InvalidOperationException($"Failed to generate URL: {result.ErrorMessage}");
    }

    public async Task<string> GetUploadUrlAsync(string key, int expiryMinutes = 15)
    {
        var result = await _s3Service.GeneratePresignedUploadUrlAsync(key, TimeSpan.FromMinutes(expiryMinutes));
        
        return result.Success ? result.Url : throw new InvalidOperationException(result.ErrorMessage);
    }
}
```

### File Management

```csharp
public class FileManagerService
{
    private readonly IS3Service _s3Service;

    public FileManagerService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<bool> FileExistsAsync(string key)
    {
        return await _s3Service.FileExistsAsync(key);
    }

    public async Task DeleteFileAsync(string key)
    {
        var result = await _s3Service.DeleteFileAsync(key);
        
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to delete file: {result.ErrorMessage}");
        }
    }

    public async Task<List<string>> ListFilesAsync(string prefix = "", bool recursive = true)
    {
        var result = await _s3Service.ListObjectsAsync(prefix, recursive);
        
        if (result.Success)
        {
            return result.Objects.Select(obj => obj.Key).ToList();
        }
        
        return new List<string>();
    }

    public async Task CopyFileAsync(string sourceKey, string destinationKey)
    {
        var result = await _s3Service.CopyObjectAsync(sourceKey, destinationKey);
        
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to copy file: {result.ErrorMessage}");
        }
    }
}
```

### LocalStack Development Setup

#### Using Docker Compose

Create a `docker-compose.localstack.yml` file:

```yaml
version: '3.8'
services:
  localstack:
    image: localstack/localstack:latest
    container_name: localstack-s3
    ports:
      - "4566:4566"
    environment:
      - SERVICES=s3
      - DEBUG=1
      - AWS_DEFAULT_REGION=us-east-1
      - AWS_ACCESS_KEY_ID=test
      - AWS_SECRET_ACCESS_KEY=test
    volumes:
      - "./localstack:/tmp/localstack"
      - "/var/run/docker.sock:/var/run/docker.sock"
```

Start LocalStack:

```bash
docker-compose -f docker-compose.localstack.yml up -d
```

#### Standalone Docker

```bash
docker run --rm -d \
  --name localstack-s3 \
  -p 4566:4566 \
  -e SERVICES=s3 \
  -e DEBUG=1 \
  -e AWS_DEFAULT_REGION=us-east-1 \
  -e AWS_ACCESS_KEY_ID=test \
  -e AWS_SECRET_ACCESS_KEY=test \
  localstack/localstack:latest
```

## Integration with MinimalAPI

```csharp
using MinimalAPI.Endpoints;
using Services.AWS.S3;

[ApiVersion(1)]
public class UploadFileEndpoint(IS3Service s3Service) : EndpointBase
{
    [HttpPost("/api/files/upload")]
    public async Task<IResult> HandleAsync(IFormFile file, string? folder = null)
    {
        if (file.Length == 0)
        {
            return Results.BadRequest("No file provided");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var key = $"{folder ?? "uploads"}/{Guid.NewGuid()}-{file.FileName}";
            
            var result = await s3Service.UploadFileAsync(key, stream, file.ContentType);
            
            if (result.Success)
            {
                return Results.Ok(new { key = result.Key, url = await s3Service.GeneratePresignedUrlAsync(result.Key, TimeSpan.FromHours(1)) });
            }
            
            return Results.Problem(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Upload failed: {ex.Message}");
        }
    }
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Uses configuration extensions and utilities
- **Moclawr.Shared**: Integrates with response models and exception handling
- **Moclawr.Host**: Perfect for dependency injection and health checks
- **Moclawr.MinimalAPI**: Use with file upload/download endpoints
- **Moclawr.Services.Caching**: Cache file metadata and pre-signed URLs
- **Moclawr.Services.External**: Store email attachments and SMS media

## Requirements

- .NET 9.0 or higher
- AWSSDK.S3 3.7.402.8 or higher
- Microsoft.Extensions.Configuration 9.0.5 or higher
- Docker (for LocalStack development)

## License

This package is licensed under the MIT License.
