# Todo List CRUD Implementation with S3 Integration

This project implements a complete CRUD (Create, Read, Update, Delete) functionality for a Todo List application using the MinimalAPI pattern with a clean architecture approach, now including AWS S3 integration for file storage capabilities.

## Architecture

The solution follows a clean architecture pattern with the following layers:

- **Domain**: Contains the entity definitions and business rules
- **Application**: Contains use cases and business logic
- **Infrastructure**: Provides implementations for persistence and external services
- **API**: The entry point for HTTP requests with minimal endpoint definitions

## Implemented Features

- **Create Todo**: Add new todo items with title and description
- **Get Todo by ID**: Retrieve a specific todo item by its ID
- **Get All Todos**: List all todo items with pagination, sorting, and filtering
- **Update Todo**: Modify a todo item's properties, including marking as completed
- **Delete Todo**: Remove a todo item from the system
- **S3 File Storage**: Integrated AWS S3 service with LocalStack support for development

## API Versioning

The API supports comprehensive versioning through multiple configurable methods:

### Version Reading Strategies

The API can be configured to read versions using different strategies:

- **Query Parameter**: `?version=1.0` or `?version=2.0`
- **Header**: `X-Version: 1.0` or `X-Version: 2.0`  
- **URL Segment**: `/api/v1/todos` or `/api/v2/todos`
- **Media Type**: `Accept: application/json;version=1.0`

### Version Configuration

You can configure how versions are read in your `Program.cs`:

```csharp
var versioningOptions = new DefaultVersioningOptions
{
    Prefix = "v",
    DefaultVersion = 1,
    SupportedVersions = [1, 2, 3],
    
    // Configure version reading strategy
    ReadingStrategy = VersionReadingStrategy.QueryStringOrUrlSegment,
    QueryParameterName = "version",           // ?version=1.0
    VersionHeaderName = "X-API-Version",      // X-API-Version: 1.0
    MediaTypeParameterName = "version",       // application/json;version=1.0
    
    // Advanced options
    AssumeDefaultVersionWhenUnspecified = true,
    UseStrictVersionParsing = true,
    IncludeVersionInResponseHeaders = true,
    ResponseVersionHeaderName = "X-API-Version",
    AllowMultipleVersionSources = true,
    
    // Priority when multiple version sources are found
    ReadingPriority = [
        VersionReadingStrategy.UrlSegment,
        VersionReadingStrategy.Header,
        VersionReadingStrategy.QueryString,
        VersionReadingStrategy.MediaType
    ]
};
```

### Available Reading Strategy Options

| Strategy | Flag Value | Description | Example |
|----------|------------|-------------|---------|
| `QueryString` | 1 | Read from query parameter | `?version=1.0` |
| `UrlSegment` | 2 | Read from URL path | `/api/v1/todos` |
| `Header` | 4 | Read from HTTP header | `X-Version: 1.0` |
| `MediaType` | 8 | Read from Accept header | `application/json;version=1.0` |
| `QueryStringOrUrlSegment` | 3 | Combine query and URL | Both methods supported |
| `HeaderOrUrlSegment` | 6 | Combine header and URL | Both methods supported |
| `QueryStringOrHeader` | 5 | Combine query and header | Both methods supported |
| `All` | 15 | Use all strategies | All methods supported |

### Supported Versions
- **v1.0**: Initial API version
- **v2.0**: Enhanced API with additional features  
- **v3.0**: Future version (planned)

Default version is v1.0 when no version is specified and `AssumeDefaultVersionWhenUnspecified` is true.

## API Endpoints

| Method | Endpoint        | Description                                  | Versions |
|--------|-----------------|----------------------------------------------|----------|
| GET    | /api/todos      | Get all todos with optional filtering        | v1, v2   |
| GET    | /api/todos/{id} | Get a specific todo by ID                    | v1, v2   |
| POST   | /api/todos      | Create a new todo                            | v1, v2   |
| PUT    | /api/todos/{id} | Update an existing todo                      | v1, v2   |
| DELETE | /api/todos/{id} | Delete a todo                                | v1, v2   |

### Tag Management Endpoints
| pageSize     | Number of items per page                          | 10         |
| orderBy      | Property to sort by (id, title, createdat, etc.)  | "Id"       |
| isAscending  | Sort in ascending order if true                   | true       |

## Development Setup

### Prerequisites
- .NET 9.0 or higher
- Docker (for LocalStack S3 development)

### Standalone Setup (Quick Start)

For quick development without Docker, you can run the application with in-memory storage:

#### Option 1: Using Docker Compose (Recommended)
1. Start LocalStack for S3 development:
```bash
docker-compose -f docker-compose.localstack.yml up -d
```

#### Option 2: Standalone Docker
1. Run LocalStack as a standalone Docker container:
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

2. Stop LocalStack when done:
```bash
docker stop localstack-s3
```

3. The application will automatically create the S3 bucket during startup when using LocalStack.

### Configuration

The application supports both AWS S3 and LocalStack configurations:

**Development (LocalStack)** - `appsettings.Development.json`:
```json
{
  "AWS": {
    "S3": {
      "AccessKey": "test",
      "SecretKey": "test", 
      "BucketName": "todo-attachments",
      "Region": "us-east-1",
      "UseLocalStack": true,
      "LocalStackServiceUrl": "http://localhost:4566",
      "ForcePathStyle": true
    }
  }
}
```

**Production** - `appsettings.json`:
```json
{
  "AWS": {
    "S3": {
      "AccessKey": "your-access-key",
      "SecretKey": "your-secret-key",
      "BucketName": "your-bucket-name",
      "Region": "us-east-1",
      "UseLocalStack": false
    }
  }
}
```

## Testing

You can test the API using the `TodoList.http` file, which contains sample requests for all endpoints.

## Todo Item Properties

| Property    | Type       | Description                                  |
|-------------|------------|----------------------------------------------|
| id          | int        | Unique identifier                            |
| title       | string     | Title of the todo item (required)            |
| description | string     | Detailed description (optional)              |
| isCompleted | boolean    | Completion status                            |
| createdAt   | DateTime   | Creation timestamp                           |
| completedAt | DateTime?  | When the item was marked as completed        |

## S3 Integration

The application now includes AWS S3 integration for file storage capabilities. The S3 service is automatically configured and the bucket is created during application startup.

### Available S3 Operations
- File upload/download
- Pre-signed URL generation
- File existence checking
- Object listing and copying
- File deletion

### S3 API Usage Examples

#### Upload a file (with version)
```bash
curl -X POST "https://localhost:5001/api/v1/s3/upload" \
  -H "Content-Type: multipart/form-data" \
  -H "X-API-Version: 1.0" \
  -F "file=@/path/to/your/file.pdf" \
  -F "folder=documents"
```

#### Get file URL (with query version)
```bash
curl -X GET "https://localhost:5001/api/s3/url/documents/file.pdf?version=1.0&expiryMinutes=120"
```

#### List files (with header version)
```bash
curl -X GET "https://localhost:5001/api/s3/files?prefix=documents&recursive=true" \
  -H "X-API-Version: 1.0"
```

#### Download file (URL segment version)
```bash
curl -X GET "https://localhost:5001/api/v1/s3/download/documents/file.pdf" --output file.pdf
```

#### Delete file (media type version)
```bash
curl -X DELETE "https://localhost:5001/api/s3/documents/file.pdf" \
  -H "Accept: application/json;version=1.0"
```

## Implementation Details

This implementation uses:

- Entity Framework Core for data persistence
- MediatR for CQRS pattern implementation
- MinimalAPI pattern with comprehensive versioning support
- AWS S3 SDK with LocalStack support for file storage
- Flexible version reading strategies (Query, Header, URL, MediaType)
- Custom request binding for HTTP context
