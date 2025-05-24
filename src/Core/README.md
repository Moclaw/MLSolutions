# Moclawr.Core

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Core.svg)](https://www.nuget.org/packages/Moclawr.Core/)

## Overview

Moclawr.Core provides essential utility extension methods and base functionality for .NET applications. This library serves as the foundation for other Moclawr packages, offering extensions for common operations that simplify development across various .NET projects.

## Features

- **Extension Methods**: Comprehensive set of extensions for enums, objects, queryables, reflection, and strings
- **Assembly Scanning**: Tools for discovering and loading types from assemblies
- **LINQ Enhancements**: Advanced query capabilities with dynamic ordering and filtering
- **Configuration Utilities**: Helpers for working with .NET configuration

## Installation

```shell
dotnet add package Moclawr.Core
```

## Usage Examples

### Queryable Extensions

```csharp
// Dynamic ordering with string property name
var orderedCustomers = customers.OrderByDynamic("LastName", true);

// Async conversion to immutable array
var itemsArray = await queryable.ConvertToImmutableArrayAsync(cancellationToken);
```

### Enum Extensions

```csharp
// Get description attribute from enum value
var description = MyEnum.SomeValue.GetDescription();

// Convert string to enum value
var enumValue = "Active".ToEnum<Status>();
```

### String Extensions

```csharp
// Normalize a string (remove diacritics)
var normalized = "Café".NormalizeString();

// Check if string matches a wildcard pattern
bool isMatch = filename.MatchesWildcard("doc*.txt");
```

## Integration with Other Moclawr Packages

Moclawr.Core is designed to work seamlessly with other packages in the Moclawr ecosystem, particularly:

- **Moclawr.Shared**: For entity interfaces and response models
- **Moclawr.Domain**: For domain modeling and business logic
- **Moclawr.EfCore** and **Moclawr.MongoDb**: For data access

## Requirements

- .NET 9.0 or higher

## License

This project is licensed under the MIT License - see the LICENSE file for details.
