# MLSolutions Overview

This directory contains the core source code for the MLSolutions project, organized into several main modules:

## Setup 
- This is a Bash/PowerShell script that automatically sets up a Clean Architecture project structure for .NET applications, following Domain-Driven Design (DDD) and optional Test-Driven Development (TDD) standards [Script Setup](https://github.com/Moclaw/ScriptKids/tree/main/src/CleanArchitecture). 

## 1. Core
- **Extensions**: Provides utility extension methods for enums, objects, queryables, reflection, and strings. These extensions simplify common operations such as enum metadata retrieval, object serialization, dynamic query ordering, and string normalization.

## 2. Infrastructure
- **Domain**: Defines base entity types and builder interfaces for entity construction and query building. Includes:
  - `BaseEntity` and `TrackEntity` for entity modeling and audit tracking.
  - Fluent builder interfaces for constructing queries and setting entity properties.
- **ORM**: Contains data access logic for both Entity Framework Core (EfCore) and MongoDB:
  - **EfCore**: Base context, repositories, and builders for relational data access, supporting advanced query building, Dapper integration, and transaction management.
  - **MongoDb**: Context and repositories for MongoDB, supporting similar repository patterns as EfCore.
- **Services**: Provides service-level utilities and configurations:
  - **Configurations**: Module configuration and cache definitions.
  - **Constants**: Cache types and key search operators.
  - **Redis**: Redis cache abstraction and implementation, supporting advanced cache operations and key management.

## 3. Shard
- **Entities**: Base interfaces for entity modeling.
- **Exceptions**: Custom exception types for business logic, entity conflicts, and not-found scenarios.
- **Responses**: Standardized response interfaces and records for API/service responses.
- **Settings**: Default JSON serialization settings.
- **Utils**: Utility classes for object manipulation, paging, password hashing, and response creation.

## Key Features
- **Extensible Query Building**: Fluent interfaces and builders for constructing complex queries with support for navigation properties, ordering, and filtering.
- **Repository Pattern**: Unified repository interfaces for both relational (EfCore) and NoSQL (MongoDB) data sources.
- **Advanced Caching**: Redis and in-memory cache support with flexible key management and removal strategies.
- **Standardized Responses**: Consistent response types for success, error, and collection results.
- **Robust Utilities**: Helpers for serialization, password security, paging, and more.

## Usage
- Use the extension methods in `Core/Extensions` to simplify common .NET operations.
- Implement domain entities by inheriting from `BaseEntity` or `TrackEntity`.
- Use the repository interfaces in `Infrastructure/ORM` for data access abstraction.
- Configure caching and module settings via `Services/Configurations`.
- Handle errors and responses using types in `Shard/Exceptions` and `Shard/Responses`.

---
This structure enables scalable, maintainable, and testable application development for .NET projects with support for both relational and NoSQL databases, advanced caching, and robust domain modeling.
