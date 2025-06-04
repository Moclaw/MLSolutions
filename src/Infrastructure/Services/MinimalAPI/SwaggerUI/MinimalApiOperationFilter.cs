using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MinimalAPI.SwaggerUI;

public class MinimalApiOperationFilter(SwaggerUIOptions options) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Find the endpoint type for this operation
        var endpointType = FindEndpointType(context);
        if (endpointType == null)
        {
            // If we can't find endpoint type, try to generate basic parameters from route
            GenerateFallbackParameters(operation, context);
            return;
        }

        var requestType = GetRequestType(endpointType);
        if (requestType == null)
        {
            // If we can't find request type, generate basic parameters
            GenerateFallbackParameters(operation, context);
            return;
        }

        var isCommandRequest = IsCommandRequest(requestType);
        var isQueryRequest = IsQueryRequest(requestType);

        if (isQueryRequest && isCommandRequest)
        {
            return; // Cannot be both command and query
        }

        // Generate unique operation ID to prevent duplicates
        GenerateUniqueOperationId(operation, endpointType, context);

        // Generate tags based on endpoint
        GenerateOperationTags(operation, endpointType);

        // Process parameters from request type
        ProcessRequestParameters(operation, requestType, isCommandRequest, isQueryRequest, context);
    }

    private static void GenerateFallbackParameters(
        OpenApiOperation operation,
        OperationFilterContext context
    )
    {
        // Generate basic parameters based on route pattern
        var route = context.ApiDescription.RelativePath ?? "";
        var routeParts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

        operation.Parameters ??= [];

        foreach (var part in routeParts)
        {
            // If part contains {}, it's a route parameter
            if (part.Contains('{') && part.Contains('}'))
            {
                var paramName = part.Trim('{', '}');

                var parameter = new OpenApiParameter
                {
                    Name = paramName,
                    Required = true,
                    Description = $"Route parameter {paramName}",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Path,
                    Schema = new OpenApiSchema { Type = "string" },
                };

                operation.Parameters.Add(parameter);
            }
        }

        // Add some common query parameters for testing
        if (context.ApiDescription.HttpMethod?.ToUpper() == "GET")
        {
            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "page",
                    Required = false,
                    Description = "Page number for pagination",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Format = "int32",
                        Default = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                    },
                }
            );

            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "size",
                    Required = false,
                    Description = "Page size for pagination",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Format = "int32",
                        Default = new Microsoft.OpenApi.Any.OpenApiInteger(10),
                    },
                }
            );
        }
    }

    private static int ExtractVersionFromContext(OperationFilterContext context)
    {
        var route = context.ApiDescription.RelativePath ?? "";

        // Try to extract version from route first
        var routeParts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in routeParts)
        {
            if (
                part.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(part.Substring(1), out var version)
            )
            {
                return version;
            }
        }

        // Fallback to document name if available
        var documentName = context.DocumentName;
        if (
            !string.IsNullOrEmpty(documentName)
            && documentName.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(documentName.Substring(1), out var docVersion)
        )
        {
            return docVersion;
        }

        return 1; // Default to version 1
    }

    private static bool IsEndpointCompatibleWithVersion(Type endpointType, int version)
    {
        // Always return true to ensure endpoints are processed
        // Version filtering should be handled by document filter
        return true;
    }

    private void GenerateUniqueOperationId(
        OpenApiOperation operation,
        Type endpointType,
        OperationFilterContext context
    )
    {
        var feature = ExtractFeatureName(endpointType);
        var endpointName = endpointType.Name.Replace("Endpoint", "");
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? "GET";
        var route = context.ApiDescription.RelativePath ?? "";

        // Include route segments in operation ID to make it more unique
        var routeSegments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var routeIdentifier = string.Empty;

        foreach (var segment in routeSegments)
        {
            if (
                !segment.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && !segment.Equals("api", StringComparison.OrdinalIgnoreCase)
                && !segment.Contains("{")
            )
            {
                routeIdentifier += $"{char.ToUpperInvariant(segment[0])}{segment.Substring(1)}";
            }
        }

        if (string.IsNullOrEmpty(routeIdentifier))
        {
            routeIdentifier = "Api";
        }

        // Create a deterministic GUID based on the combination
        var input = $"{endpointType.FullName}_{httpMethod}_{route}";
        var hash = CreateDeterministicGuid(input);
        var shortHash = hash[..8]; // First 8 characters

        operation.OperationId =
            $"{feature}_{endpointName}_{routeIdentifier}_{httpMethod}_{shortHash}";
    }

    private static string CreateDeterministicGuid(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Use first 16 bytes to create a GUID
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);

        return new Guid(guidBytes).ToString("N");
    }

    private static string ExtractFeatureFromRoute(string route)
    {
        var parts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Find parts that might represent features (ignoring api, v1, etc.)
        foreach (var part in parts)
        {
            // Skip common API path elements
            if (
                part.Equals("api", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty(part)
                || part.Contains("{")
            )
                continue;

            return part;
        }

        return string.Empty;
    }

    private static string ExtractFeatureName(Type endpointType)
    {
        // Try to extract from namespace first
        var namespaceParts = endpointType.Namespace?.Split('.') ?? [];
        var endpointsIndex = Array.FindIndex(
            namespaceParts,
            part => part.Equals("Endpoints", StringComparison.OrdinalIgnoreCase)
        );

        if (endpointsIndex >= 0 && endpointsIndex + 1 < namespaceParts.Length)
        {
            var featureName = namespaceParts[endpointsIndex + 1];

            // Convert kebab or snake case to proper case
            if (featureName.Contains('-'))
            {
                // Handle kebab-case feature names
                var parts = featureName.Split('-');
                featureName = string.Join(
                    "",
                    parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1))
                );
            }

            return featureName;
        }

        // Fallback to class name
        var typeName = endpointType.Name;
        if (typeName.EndsWith("Endpoint"))
        {
            typeName = typeName.Substring(0, typeName.Length - 8);
        }

        // Try to extract feature name from class name
        if (typeName.Contains("Command") || typeName.Contains("Query"))
        {
            // For command/query handlers, the feature is typically before Command/Query
            var featurePart = typeName.Split(new[] { "Command", "Query" }, StringSplitOptions.None)[
                0
            ];
            return featurePart;
        }

        return typeName;
    }

    private static bool IsCommandRequest(Type? requestType)
    {
        if (requestType == null)
            return false;

        return requestType
            .GetInterfaces()
            .Any(i =>
                i == typeof(ICommand)
                || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
            );
    }

    private static bool IsQueryRequest(Type? requestType)
    {
        if (requestType == null)
            return false;

        return requestType
            .GetInterfaces()
            .Any(i =>
                i == typeof(IQueryRequest)
                || (
                    i.IsGenericType
                    && (
                        i.GetGenericTypeDefinition() == typeof(IQueryRequest<>)
                        || i.GetGenericTypeDefinition() == typeof(IQueryCollectionRequest<>)
                    )
                )
            );
    }

    private void GenerateOperationTags(OpenApiOperation operation, Type endpointType)
    {
        var tags = new List<string>();

        // Extract feature from namespace
        var namespaceParts = endpointType.Namespace?.Split('.') ?? Array.Empty<string>();

        // Find "Endpoints" part in namespace
        var endpointsIndex = Array.FindIndex(
            namespaceParts,
            part => part.Equals("Endpoints", StringComparison.OrdinalIgnoreCase)
        );

        if (endpointsIndex >= 0 && endpointsIndex + 1 < namespaceParts.Length)
        {
            var featureName = namespaceParts[endpointsIndex + 1]; // e.g., "S3", "Todos", "AutofacDemo"

            // Convert kebab-case to pascal case
            if (featureName.Contains('-'))
            {
                featureName = ToPascalCase(featureName);
            }

            // Add operation type if available (Commands, Queries, etc.)
            if (endpointsIndex + 2 < namespaceParts.Length)
            {
                var operationType = namespaceParts[endpointsIndex + 2]; // e.g., "Commands", "Queries"
                tags.Add($"{featureName} {operationType}");
            }
            else
            {
                // Determine if it's a command or query from the handler type
                bool isCommand = IsCommandEndpoint(endpointType);
                bool isQuery = IsQueryEndpoint(endpointType);

                if (isCommand)
                {
                    tags.Add($"{featureName} Commands");
                }
                else if (isQuery)
                {
                    tags.Add($"{featureName} Queries");
                }
                else
                {
                    // Just use the feature name if there's no operation type
                    tags.Add(featureName);
                }
            }
        }
        else
        {
            // Fallback to class name
            string typeName = endpointType.Name;
            if (typeName.EndsWith("Endpoint"))
            {
                typeName = typeName.Substring(0, typeName.Length - 8);
            }

            tags.Add(typeName);
        }

        operation.Tags = tags.Select(tag => new OpenApiTag { Name = tag }).ToList();
    }

    private static bool IsCommandEndpoint(Type endpointType)
    {
        // Check if the endpoint name contains "Command"
        if (endpointType.Name.Contains("Command"))
            return true;

        // Check methods for [HttpPost], [HttpPut], [HttpDelete], etc.
        var methods = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            var attrs = method.GetCustomAttributes();
            if (
                attrs.Any(a =>
                    a is Attributes.HttpPostAttribute
                    || a is Attributes.HttpPutAttribute
                    || a is Attributes.HttpDeleteAttribute
                    || a is Attributes.HttpPatchAttribute
                )
            )
                return true;
        }

        // Check if the request type implements ICommand
        var requestType = GetRequestType(endpointType);
        return IsCommandRequest(requestType);
    }

    private static bool IsQueryEndpoint(Type endpointType)
    {
        // Check if the endpoint name contains "Query"
        if (endpointType.Name.Contains("Query"))
            return true;

        // Check methods for [HttpGet]
        var methods = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            var attrs = method.GetCustomAttributes();
            if (attrs.Any(a => a is Attributes.HttpGetAttribute))
                return true;
        }

        // Check if the request type implements IQueryRequest
        var requestType = GetRequestType(endpointType);
        return IsQueryRequest(requestType);
    }

    private static Type? GetRequestType(Type endpointType)
    {
        // Check all base types to find the generic one
        var currentType = endpointType;
        while (currentType != null)
        {
            if (currentType.IsGenericType)
            {
                var genericArgs = currentType.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    // First generic argument should be the request type
                    return genericArgs[0];
                }
            }
            currentType = currentType.BaseType;
        }

        // Alternative: look for HandleAsync method parameter
        var handleMethod = endpointType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "HandleAsync");

        if (handleMethod != null)
        {
            var parameters = handleMethod.GetParameters();
            if (parameters.Length > 0)
            {
                return parameters[0].ParameterType;
            }
        }

        return null;
    }

    private static void ProcessRequestParameters(
        OpenApiOperation operation,
        Type requestType,
        bool isCommandRequest,
        bool isQueryRequest,
        OperationFilterContext context
    )
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? "GET";

        // Initialize parameters collection
        operation.Parameters ??= [];

        // For POST/PUT operations, default to request body
        var shouldUseRequestBody =
            httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "PATCH";

        // Check if this is a form data request (has IFormFile or FromForm attributes)
        var hasFormData = properties.Any(p =>
            p.PropertyType == typeof(IFormFile)
            || p.PropertyType == typeof(IFormFile[])
            || p.PropertyType == typeof(List<IFormFile>)
            || p.GetCustomAttribute<Attributes.FromFormAttribute>() != null
        );

        var bodyProperties = new List<PropertyInfo>();
        var formProperties = new List<PropertyInfo>();
        var hasAnyParameters = false;

        foreach (var property in properties)
        {
            // Skip service injection properties
            if (property.GetCustomAttribute<Attributes.FromServicesAttribute>() != null)
                continue;

            var parameterLocation = DetermineParameterLocation(
                property,
                isCommandRequest,
                isQueryRequest,
                hasFormData,
                shouldUseRequestBody
            );

            // Handle different parameter locations
            switch (parameterLocation)
            {
                case Attributes.ParameterLocation.Path:
                case Attributes.ParameterLocation.Query:
                case Attributes.ParameterLocation.Header:
                case Attributes.ParameterLocation.Cookie:
                    var parameter = CreateParameterFromProperty(property, parameterLocation);
                    if (parameter != null)
                    {
                        // Remove any existing parameter with the same name to prevent duplicates
                        var existing = operation.Parameters.FirstOrDefault(p =>
                            p.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)
                        );
                        if (existing != null)
                        {
                            operation.Parameters.Remove(existing);
                        }

                        operation.Parameters.Add(parameter);
                        hasAnyParameters = true;
                    }
                    break;

                case Attributes.ParameterLocation.Body:
                    bodyProperties.Add(property);
                    break;

                case Attributes.ParameterLocation.Form:
                    formProperties.Add(property);
                    break;
            }
        }

        // Generate request body if needed
        if (formProperties.Any())
        {
            GenerateFormDataRequestBody(operation, requestType, formProperties);
        }
        else if (bodyProperties.Any() || (shouldUseRequestBody && !hasAnyParameters))
        {
            GenerateRequestBodyForCommand(
                operation,
                requestType,
                bodyProperties.Any() ? bodyProperties : null
            );
        }

        // If still no parameters and no request body, generate some basic ones for testing
        if (!hasAnyParameters && operation.Parameters.Count == 0 && operation.RequestBody == null)
        {
            if (httpMethod == "GET")
            {
                operation.Parameters.Add(
                    new OpenApiParameter
                    {
                        Name = "id",
                        Required = false,
                        Description = "Resource identifier",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                        Schema = new OpenApiSchema { Type = "integer", Format = "int32" },
                    }
                );
            }
        }
    }

    private static MinimalAPI.Attributes.ParameterLocation DetermineParameterLocation(
        PropertyInfo property,
        bool isCommandRequest,
        bool isQueryRequest,
        bool hasFormData,
        bool shouldUseRequestBody = false
    )
    {
        // Check for explicit binding attributes first
        if (property.GetCustomAttribute<Attributes.FromRouteAttribute>() != null)
            return Attributes.ParameterLocation.Path;
        if (property.GetCustomAttribute<Attributes.FromQueryAttribute>() != null)
            return Attributes.ParameterLocation.Query;
        if (property.GetCustomAttribute<Attributes.FromHeaderAttribute>() != null)
            return Attributes.ParameterLocation.Header;
        if (property.GetCustomAttribute<Attributes.FromBodyAttribute>() != null)
            return Attributes.ParameterLocation.Body;
        if (property.GetCustomAttribute<Attributes.FromFormAttribute>() != null)
            return Attributes.ParameterLocation.Form;

        // Handle IFormFile types - these should always be form parameters
        if (
            property.PropertyType == typeof(IFormFile)
            || property.PropertyType == typeof(IFormFile[])
            || property.PropertyType == typeof(List<IFormFile>)
        )
        {
            return Attributes.ParameterLocation.Form;
        }

        // Smart defaults based on HTTP method and request type
        if (isQueryRequest)
        {
            // Query requests: all properties should be query parameters unless explicitly specified
            return Attributes.ParameterLocation.Query;
        }

        if (isCommandRequest || shouldUseRequestBody)
        {
            // For command requests (POST/PUT/PATCH), check if we have form data
            if (hasFormData)
            {
                // If there's form data, non-explicit properties go to form
                return Attributes.ParameterLocation.Form;
            }
            else
            {
                // For POST/PUT/PATCH, default to request body
                return Attributes.ParameterLocation.Body;
            }
        }

        // For GET and other methods, simple types go to query parameters
        if (IsSimpleType(property.PropertyType))
        {
            return Attributes.ParameterLocation.Query;
        }

        // Default fallback - complex types go to body for POST/PUT, query for others
        return shouldUseRequestBody
            ? Attributes.ParameterLocation.Body
            : Attributes.ParameterLocation.Query;
    }

    private static bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive
            || underlyingType == typeof(string)
            || underlyingType == typeof(DateTime)
            || underlyingType == typeof(DateTimeOffset)
            || underlyingType == typeof(TimeSpan)
            || underlyingType == typeof(decimal)
            || underlyingType == typeof(Guid)
            || underlyingType.IsEnum;
    }

    private static OpenApiParameter? CreateParameterFromProperty(
        PropertyInfo property,
        MinimalAPI.Attributes.ParameterLocation location
    )
    {
        if (location == Attributes.ParameterLocation.Body)
            return null;

        var parameter = new OpenApiParameter
        {
            Name = GetParameterName(property),
            Required = IsRequiredParameter(property),
            Description = GetParameterDescription(property),
            In = MapParameterLocation(location),
            Schema = CreateSchemaForType(property.PropertyType),
        };

        // Add example if available
        var example = GetParameterExample(property);
        if (example != null)
        {
            parameter.Example = example;
        }

        return parameter;
    }

    private static string GetParameterName(PropertyInfo property)
    {
        // Check for custom name in binding attributes
        var routeAttr = property.GetCustomAttribute<Attributes.FromRouteAttribute>();
        if (routeAttr != null && !string.IsNullOrEmpty(routeAttr.Name))
            return routeAttr.Name;

        var queryAttr = property.GetCustomAttribute<Attributes.FromQueryAttribute>();
        if (queryAttr != null && !string.IsNullOrEmpty(queryAttr.Name))
            return queryAttr.Name;

        var headerAttr = property.GetCustomAttribute<Attributes.FromHeaderAttribute>();
        if (headerAttr != null && !string.IsNullOrEmpty(headerAttr.Name))
            return headerAttr.Name;

        return property.Name;
    }

    private static bool IsRequiredParameter(PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        // Check for nullable reference types using NullabilityInfo
        var nullabilityContext = new System.Reflection.NullabilityInfoContext();
        var nullabilityInfo = nullabilityContext.Create(property);

        // If it's a nullable reference type or nullable value type, it's not required
        if (nullabilityInfo.WriteState == System.Reflection.NullabilityState.Nullable)
            return false;

        // Value types are required unless they are Nullable<T>
        if (propertyType.IsValueType)
            return Nullable.GetUnderlyingType(propertyType) == null;

        // Reference types are required by default (unless nullable)
        return true;
    }

    private static string GetParameterDescription(PropertyInfo property) =>
        $"{property.Name} parameter";

    private static Microsoft.OpenApi.Any.IOpenApiAny? GetParameterExample(PropertyInfo property)
    {
        var propertyType =
            Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (propertyType == typeof(string))
            return new Microsoft.OpenApi.Any.OpenApiString("example");
        if (propertyType == typeof(int))
            return new Microsoft.OpenApi.Any.OpenApiInteger(1);
        if (propertyType == typeof(bool))
            return new Microsoft.OpenApi.Any.OpenApiBoolean(true);

        return null;
    }

    private static Microsoft.OpenApi.Models.ParameterLocation? MapParameterLocation(
        MinimalAPI.Attributes.ParameterLocation location
    ) =>
        location switch
        {
            Attributes.ParameterLocation.Query => Microsoft.OpenApi.Models.ParameterLocation.Query,
            Attributes.ParameterLocation.Path => Microsoft.OpenApi.Models.ParameterLocation.Path,
            Attributes.ParameterLocation.Header => Microsoft
                .OpenApi
                .Models
                .ParameterLocation
                .Header,
            Attributes.ParameterLocation.Cookie => Microsoft
                .OpenApi
                .Models
                .ParameterLocation
                .Cookie,
            Attributes.ParameterLocation.Form => null, // Form parameters are handled in request body
            Attributes.ParameterLocation.Body => null, // Body parameters are handled in request body
            _ => Microsoft.OpenApi.Models.ParameterLocation.Query,
        };

    private static OpenApiSchema CreateSchemaForType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        var schema = new OpenApiSchema();

        if (underlyingType == typeof(string))
        {
            schema.Type = "string";
        }
        else if (underlyingType == typeof(int))
        {
            schema.Type = "integer";
            schema.Format = "int32";
        }
        else if (underlyingType == typeof(long))
        {
            schema.Type = "integer";
            schema.Format = "int64";
        }
        else if (underlyingType == typeof(bool))
        {
            schema.Type = "boolean";
        }
        else if (underlyingType == typeof(DateTime))
        {
            schema.Type = "string";
            schema.Format = "date-time";
        }
        else if (
            underlyingType == typeof(decimal)
            || underlyingType == typeof(double)
            || underlyingType == typeof(float)
        )
        {
            schema.Type = "number";
        }
        else if (underlyingType.IsEnum)
        {
            schema.Type = "string";
            schema.Enum = [];
            foreach (var enumName in Enum.GetNames(underlyingType))
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumName));
            }
        }
        else if (
            underlyingType.IsGenericType
            && underlyingType.GetGenericTypeDefinition() == typeof(List<>)
        )
        {
            schema.Type = "array";
            var itemType = underlyingType.GetGenericArguments()[0];
            schema.Items = CreateSchemaForType(itemType);
        }

        if (type != underlyingType)
        {
            schema.Nullable = true;
        }

        return schema;
    }

    private static void GenerateRequestBodyForCommand(
        OpenApiOperation operation,
        Type requestType,
        List<PropertyInfo>? bodyProperties = null
    )
    {
        var properties = bodyProperties;

        // If no specific body properties provided, include all properties that should be in body
        if (bodyProperties == null)
        {
            var allProperties = requestType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            );
            properties = allProperties
                .Where(prop =>
                    prop.GetCustomAttribute<Attributes.FromServicesAttribute>() == null
                    && !HasExplicitNonBodyBinding(prop)
                )
                .ToList();
        }

        if (properties == null || properties.Count == 0)
            return;

        var requestBodyProperties = new Dictionary<string, OpenApiSchema>();
        var requiredProperties = new HashSet<string>();

        foreach (var property in properties)
        {
            var propertySchema = CreateSchemaForType(property.PropertyType);
            requestBodyProperties[property.Name] = propertySchema;

            if (IsRequiredParameter(property))
            {
                requiredProperties.Add(property.Name);
            }
        }

        if (requestBodyProperties.Count > 0)
        {
            var requestBodySchema = new OpenApiSchema
            {
                Type = "object",
                Properties = requestBodyProperties,
                Required = requiredProperties,
                AdditionalPropertiesAllowed = false,
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = requiredProperties.Count > 0,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = requestBodySchema },
                },
            };
        }
    }

    private static void GenerateFormDataRequestBody(
        OpenApiOperation operation,
        Type requestType,
        List<PropertyInfo>? formProperties = null
    )
    {
        var properties =
            formProperties
            ?? requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        var requestFormProperties = new Dictionary<string, OpenApiSchema>();
        var requiredProperties = new HashSet<string>();

        foreach (var property in properties)
        {
            // Skip properties with explicit non-form bindings
            if (formProperties == null && HasExplicitNonFormBinding(property))
                continue;

            OpenApiSchema propertySchema;

            // Handle IFormFile types
            if (property.PropertyType == typeof(IFormFile))
            {
                propertySchema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = $"File upload for {property.Name}",
                };
            }
            else if (
                property.PropertyType == typeof(IFormFile[])
                || property.PropertyType == typeof(List<IFormFile>)
            )
            {
                propertySchema = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "string", Format = "binary" },
                    Description = $"Multiple file uploads for {property.Name}",
                };
            }
            else
            {
                propertySchema = CreateSchemaForType(property.PropertyType);
            }

            requestFormProperties[property.Name] = propertySchema;

            if (IsRequiredParameter(property))
            {
                requiredProperties.Add(property.Name);
            }
        }

        if (requestFormProperties.Count > 0)
        {
            var requestBodySchema = new OpenApiSchema
            {
                Type = "object",
                Properties = requestFormProperties,
                Required = requiredProperties,
                AdditionalPropertiesAllowed = false,
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = requiredProperties.Count > 0,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType { Schema = requestBodySchema },
                },
            };
        }
    }

    private static bool HasExplicitNonBodyBinding(PropertyInfo property) =>
        property.GetCustomAttribute<Attributes.FromRouteAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromQueryAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromHeaderAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromServicesAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromBodyAttribute>() != null;

    private static bool HasExplicitNonFormBinding(PropertyInfo property) =>
        property.GetCustomAttribute<Attributes.FromRouteAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromQueryAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromHeaderAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromServicesAttribute>() != null
        || property.GetCustomAttribute<Attributes.FromBodyAttribute>() != null;

    private Type? FindEndpointType(OperationFilterContext context)
    {
        if (options.EndpointAssemblies == null || options.EndpointAssemblies.Length == 0)
            return null;

        var endpointTypes = options
            .EndpointAssemblies.SelectMany(a => a.GetTypes())
            .Where(t =>
                !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(EndpointAbstractBase))
            )
            .ToList();

        var contextRoute = context.ApiDescription.RelativePath ?? "";
        var contextMethod = context.ApiDescription.HttpMethod ?? "";

        // First, try to use the contextRoute directly to find endpoints with matching route attributes
        var routeMatches = FindEndpointsByRoutePattern(endpointTypes, contextRoute, contextMethod);
        if (routeMatches.Count > 0)
        {
            return routeMatches[0];
        }

        // Extract feature name from route (e.g., "autofac-demo" from "/api/v1/autofac-demo/...")
        string featureFromRoute = ExtractFeatureFromRoute(contextRoute);

        if (!string.IsNullOrEmpty(featureFromRoute))
        {
            // Try different variations of the feature name in case of kebab-cased routes
            var featureVariants = new List<string>
            {
                featureFromRoute, // Original: "autofac-demo"
                featureFromRoute.Replace("-", ""), // No hyphens: "autofacdemo"
                ToPascalCase(featureFromRoute), // PascalCase: "AutofacDemo"
                featureFromRoute.Replace("-", "_"), // With underscores: "autofac_demo"
            };

            // Try to match by namespace first - most reliable method
            foreach (var variant in featureVariants)
            {
                var matchByNamespace = endpointTypes.FirstOrDefault(t =>
                    t.Namespace?.Contains($".{variant}.", StringComparison.OrdinalIgnoreCase)
                        == true
                    || t.Namespace?.EndsWith($".{variant}", StringComparison.OrdinalIgnoreCase)
                        == true
                );

                if (matchByNamespace != null)
                {
                    // Verify HTTP method matches
                    var methodMatches = VerifyHttpMethodMatch(matchByNamespace, contextMethod);
                    if (methodMatches)
                        return matchByNamespace;
                }
            }
        }

        // Extract path segments for matching
        var routeSegments = contextRoute
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(s =>
                !s.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && !s.Equals("api", StringComparison.OrdinalIgnoreCase)
                && !s.Contains("{")
            )
            .ToList();

        // Try to match resource name with endpoint name
        if (routeSegments.Count > 0)
        {
            var resourceName = routeSegments.Last();
            var candidateEndpoints = endpointTypes
                .Where(t =>
                    t.Name.Replace("Endpoint", "")
                        .Equals(resourceName, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            foreach (var endpoint in candidateEndpoints)
            {
                if (VerifyHttpMethodMatch(endpoint, contextMethod))
                {
                    return endpoint;
                }
            }
        }

        // Last resort: try each endpoint type and check HTTP method
        foreach (var endpointType in endpointTypes)
        {
            if (VerifyHttpMethodMatch(endpointType, contextMethod))
            {
                return endpointType;
            }
        }

        return null;
    }

    private static bool VerifyHttpMethodMatch(Type endpointType, string contextMethod)
    {
        var methods = endpointType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes().Any(a => IsHttpMethodAttribute(a)))
            .ToList();

        foreach (var method in methods)
        {
            var httpMethodAttr =
                method.GetCustomAttributes().FirstOrDefault(a => IsHttpMethodAttribute(a))
                as HttpMethodAttribute;

            if (
                httpMethodAttr?.Method != null
                && httpMethodAttr.Method.Equals(contextMethod, StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsHttpMethodAttribute(Attribute attribute)
    {
        var typeName = attribute.GetType().Name;
        return typeName == "HttpGetAttribute"
            || typeName == "HttpPostAttribute"
            || typeName == "HttpPutAttribute"
            || typeName == "HttpDeleteAttribute"
            || typeName == "HttpPatchAttribute";
    }

    private static List<Type> FindEndpointsByRoutePattern(
        List<Type> endpointTypes,
        string contextRoute,
        string contextMethod
    )
    {
        var matches = new List<Type>();
        foreach (var endpointType in endpointTypes)
        {
            var methods = endpointType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes().Any(a => IsHttpMethodAttribute(a)))
                .ToList();

            foreach (var method in methods)
            {
                if (
                    method.GetCustomAttributes().FirstOrDefault(a => IsHttpMethodAttribute(a))
                        is not HttpMethodAttribute httpAttr
                    || httpAttr.Method != contextMethod
                )
                    continue;

                var routeTemplate = httpAttr.Route ?? "";

                // Construct the full route pattern
                var fullRoute = GetFullRoutePattern(endpointType, routeTemplate);

                if (RouteMatchesContext(fullRoute, contextRoute))
                {
                    matches.Add(endpointType);
                    break;
                }
            }
        }
        return matches;
    }

    private static bool RouteMatchesContext(string endpointRoute, string contextRoute)
    {
        // Normalize routes for comparison
        endpointRoute = NormalizeRoute(endpointRoute);
        contextRoute = NormalizeRoute(contextRoute);

        var endpointParts = endpointRoute.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var contextParts = contextRoute.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Skip version and api segments in matching
        endpointParts = endpointParts
            .Where(p =>
                !p.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && !p.Equals("api", StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

        contextParts = contextParts
            .Where(p =>
                !p.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && !p.Equals("api", StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

        // Match route parameters (segments with {})
        if (endpointParts.Length != contextParts.Length)
            return false;

        for (int i = 0; i < endpointParts.Length; i++)
        {
            var endpointPart = endpointParts[i];
            var contextPart = contextParts[i];

            // If part has route parameter, consider it a match
            if (endpointPart.Contains("{") || contextPart.Contains("{"))
                continue;

            // Otherwise parts should match (check for case insensitive equality or contained text)
            if (
                !endpointPart.Equals(contextPart, StringComparison.OrdinalIgnoreCase)
                && !endpointPart.Contains(contextPart, StringComparison.OrdinalIgnoreCase)
                && !contextPart.Contains(endpointPart, StringComparison.OrdinalIgnoreCase)
            )
            {
                return false;
            }
        }

        return true;
    }

    private static string GetFullRoutePattern(Type endpointType, string methodRoute)
    {
        // First check for route attribute on the endpoint class
        // Using regular RouteAttribute since it's not in MinimalAPI.Attributes
        var routeAttrs = endpointType.GetCustomAttributes<Microsoft.AspNetCore.Mvc.RouteAttribute>(
            true
        );
        var classRoute = "";

        foreach (var attr in routeAttrs)
        {
            if (!string.IsNullOrEmpty(attr.Template))
            {
                classRoute = attr.Template.Trim('/');
                break;
            }
        }

        // Also check for MinimalAPI specific RouteAttribute if it exists
        if (string.IsNullOrEmpty(classRoute))
        {
            var customRouteAttrs = endpointType
                .GetCustomAttributes(true)
                .Where(a => a.GetType().Name == "RouteAttribute")
                .ToList();

            if (customRouteAttrs.Count > 0)
            {
                // Use reflection to get the Pattern property
                var patternProp = customRouteAttrs[0].GetType().GetProperty("Pattern");
                if (patternProp != null)
                {
                    var pattern = patternProp.GetValue(customRouteAttrs[0]) as string;
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        classRoute = pattern.Trim('/');
                    }
                }
            }
        }

        // Combine class route with method route if both exist
        if (!string.IsNullOrEmpty(classRoute) && !string.IsNullOrEmpty(methodRoute))
        {
            return $"{classRoute}/{methodRoute}";
        }

        return !string.IsNullOrEmpty(methodRoute) ? methodRoute : classRoute;
    }

    private static string NormalizeRoute(string route)
    {
        // Remove leading slash and convert to lowercase
        var normalized = route.TrimStart('/').ToLower();

        // Keep route parameters for better matching
        return normalized;
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var parts = input.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries);
        var result = string.Join(
            "",
            parts.Select(p =>
                string.IsNullOrEmpty(p)
                    ? ""
                    : char.ToUpperInvariant(p[0]) + p.Substring(1).ToLowerInvariant()
            )
        );

        return result;
    }
}
