# Todo List CRUD Implementation

This project implements a complete CRUD (Create, Read, Update, Delete) functionality for a Todo List application using the MinimalAPI pattern with a clean architecture approach.

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

## API Endpoints

| Method | Endpoint        | Description                                  |
|--------|-----------------|----------------------------------------------|
| GET    | /api/todos      | Get all todos with optional filtering        |
| GET    | /api/todos/{id} | Get a specific todo by ID                    |
| POST   | /api/todos      | Create a new todo                            |
| PUT    | /api/todos/{id} | Update an existing todo                      |
| DELETE | /api/todos/{id} | Delete a todo                                |

## Query Parameters for GET /api/todos

| Parameter    | Description                                       | Default    |
|--------------|---------------------------------------------------|------------|
| search       | Filter todos by title or description              | null       |
| pageIndex    | Zero-based page index                             | 0          |
| pageSize     | Number of items per page                          | 10         |
| orderBy      | Property to sort by (id, title, createdat, etc.)  | "Id"       |
| isAscending  | Sort in ascending order if true                   | true       |

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

## Implementation Details

This implementation uses:

- Entity Framework Core for data persistence
- MediatR for CQRS pattern implementation
- MinimalAPI pattern for lightweight endpoints
- Custom request binding for HTTP context
