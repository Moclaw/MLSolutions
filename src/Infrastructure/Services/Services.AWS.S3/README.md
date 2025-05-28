# Services.AWS.S3

AWS S3 service integration for MLSolutions projects.

## Overview

This package provides a simple and consistent way to interact with AWS S3 storage in .NET applications. It includes features for uploading, downloading, and managing files in S3 buckets.

## Features

- File Upload/Download
- File Deletion
- Pre-signed URL Generation
- Object Listing with Recursive Support
- Object Copy between Paths/Buckets
- File Existence Check
- Configurable AWS Credentials (Direct or from AWS Credentials file)

## Installation

The package is available as a NuGet package. Add it to your project using:

```shell
dotnet add package Services.AWS.S3
```

## Configuration

Add the following configuration in your `appsettings.json`:

```json
{
  "S3Configuration": {
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "BucketName": "your-bucket-name",
    "Region": "us-east-1",
    "UseCredentialsFile": false,
    "CredentialsFilePath": ""
  }
}
```

If you're using AWS credentials file, set `UseCredentialsFile` to true and optionally specify the `CredentialsFilePath`.

## Usage

### Service Registration

Register the S3 service in your `Program.cs` or `Startup.cs`:

```csharp
using Services.AWS.S3.Extensions;

// Add S3 services
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

## License

This project is licensed under the MIT License - see the LICENSE file for details.
