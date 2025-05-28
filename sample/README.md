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

## API Endpoints

| Method | Endpoint        | Description                                  |
|--------|-----------------|----------------------------------------------|
| GET    | /api/todos      | Get all todos with optional filtering        |
| GET    | /api/todos/{id} | Get a specific todo by ID                    |
| POST   | /api/todos      | Create a new todo                            |
| PUT    | /api/todos/{id} | Update an existing todo                      |
| DELETE | /api/todos/{id} | Delete a todo                                |

### S3 File Storage Endpoints

| Method | Endpoint              | Description                                  |
|--------|-----------------------|----------------------------------------------|
| POST   | /api/s3/upload        | Upload a file to S3 storage                 |
| GET    | /api/s3/url/{key}     | Get pre-signed URL for file access          |
| GET    | /api/s3/files         | List files in S3 bucket                     |
| GET    | /api/s3/download/{key}| Download a file from S3                     |
| DELETE | /api/s3/{key}         | Delete a file from S3                       |

## Query Parameters for GET /api/todos

| Parameter    | Description                                       | Default    |
|--------------|---------------------------------------------------|------------|
| search       | Filter todos by title or description              | null       |
| pageIndex    | Zero-based page index                             | 0          |
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

#### Upload a file
```bash
curl -X POST "https://localhost:5001/api/s3/upload" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/your/file.pdf" \
  -F "folder=documents"
```

#### Get file URL
```bash
curl -X GET "https://localhost:5001/api/s3/url/documents/file.pdf?expiryMinutes=120"
```

#### List files
```bash
curl -X GET "https://localhost:5001/api/s3/files?prefix=documents&recursive=true"
```

#### Download file
```bash
curl -X GET "https://localhost:5001/api/s3/download/documents/file.pdf" --output file.pdf
```

#### Delete file
```bash
curl -X DELETE "https://localhost:5001/api/s3/documents/file.pdf"
```

## Implementation Details

This implementation uses:

- Entity Framework Core for data persistence
- MediatR for CQRS pattern implementation
- MinimalAPI pattern for lightweight endpoints
- AWS S3 SDK with LocalStack support for file storage
- Custom request binding for HTTP context
