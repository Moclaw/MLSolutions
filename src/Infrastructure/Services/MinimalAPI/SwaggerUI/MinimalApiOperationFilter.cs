using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

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
        ProcessRequestParameters(operation, requestType, isCommandRequest, isQueryRequest);
    }

    private static void GenerateFallbackParameters(OpenApiOperation operation, OperationFilterContext context)
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
                    Schema = new OpenApiSchema { Type = "string" }
                };
                
                operation.Parameters.Add(parameter);
            }
        }
        
        // Add some common query parameters for testing
        if (context.ApiDescription.HttpMethod?.ToUpper() == "GET")
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "page",
                Required = false,
                Description = "Page number for pagination",
                In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "integer", Format = "int32", Default = new Microsoft.OpenApi.Any.OpenApiInteger(1) }
            });
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "size",
                Required = false,
                Description = "Page size for pagination",
                In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "integer", Format = "int32", Default = new Microsoft.OpenApi.Any.OpenApiInteger(10) }
            });
        }
    }

    private static int ExtractVersionFromContext(OperationFilterContext context)
    {
        var route = context.ApiDescription.RelativePath ?? "";
        
        // Try to extract version from route first
        var routeParts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in routeParts)
        {
            if (part.StartsWith("v", StringComparison.OrdinalIgnoreCase) && 
                int.TryParse(part.Substring(1), out var version))
            {
                return version;
            }
        }

        // Fallback to document name if available
        var documentName = context.DocumentName;
        if (!string.IsNullOrEmpty(documentName) && 
            documentName.StartsWith("v", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(documentName.Substring(1), out var docVersion))
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

    private void GenerateUniqueOperationId(OpenApiOperation operation, Type endpointType, OperationFilterContext context)
    {
        var feature = ExtractFeatureName(endpointType);
        var endpointName = endpointType.Name.Replace("Endpoint", "");
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? "GET";
        var route = context.ApiDescription.RelativePath ?? "";

        // Create a deterministic GUID based on the combination
        var input = $"{endpointType.FullName}_{httpMethod}_{route}";
        var hash = CreateDeterministicGuid(input);
        var shortHash = hash[..8]; // First 8 characters

        operation.OperationId = $"{feature}_{endpointName}_{httpMethod}_{shortHash}";
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

    private static string ExtractFeatureName(Type endpointType)
    {
        var namespaceParts = endpointType.Namespace?.Split('.') ?? [];
        var endpointsIndex = Array.FindIndex(namespaceParts, part => 
            part.Equals("Endpoints", StringComparison.OrdinalIgnoreCase));

        if (endpointsIndex >= 0 && endpointsIndex + 1 < namespaceParts.Length)
        {
            return namespaceParts[endpointsIndex + 1];
        }

        return "Unknown";
    }

    private Type? FindEndpointType(OperationFilterContext context)
    {
        if (options.EndpointAssemblies == null || options.EndpointAssemblies.Length == 0)
            return null;

        var endpointTypes = options.EndpointAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(EndpointAbstractBase)))
            .ToList();

        var contextRoute = context.ApiDescription.RelativePath ?? "";
        var contextMethod = context.ApiDescription.HttpMethod ?? "";

        // Try simple name matching first
        var routeParts = contextRoute.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var possibleEndpointNames = new List<string>();
        
        // Extract possible endpoint names from route
        foreach (var part in routeParts)
        {
            if (!part.Contains('{') && !part.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                possibleEndpointNames.Add(part);
            }
        }

        // Try to match by endpoint name
        foreach (var endpointType in endpointTypes)
        {
            var endpointName = endpointType.Name.Replace("Endpoint", "").ToLower();
            
            foreach (var possibleName in possibleEndpointNames)
            {
                if (possibleName.ToLower().Contains(endpointName) || 
                    endpointName.Contains(possibleName.ToLower()))
                {
                    // Additional check for HTTP method match
                    var methods = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.GetCustomAttributes().Any(a => a is HttpMethodAttribute))
                        .ToList();

                    foreach (var method in methods)
                    {
                        var httpMethodAttr = method.GetCustomAttributes()
                            .FirstOrDefault(a => a is HttpMethodAttribute) as HttpMethodAttribute;

                        if (httpMethodAttr?.Method != null &&
                            httpMethodAttr.Method.Equals(contextMethod, StringComparison.OrdinalIgnoreCase))
                        {
                            return endpointType;
                        }
                    }
                }
            }
        }

        // Fallback: return first endpoint that matches HTTP method
        foreach (var endpointType in endpointTypes)
        {
            var methods = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes().Any(a => a is HttpMethodAttribute))
                .ToList();

            foreach (var method in methods)
            {
                var httpMethodAttr = method.GetCustomAttributes()
                    .FirstOrDefault(a => a is HttpMethodAttribute) as HttpMethodAttribute;

                if (httpMethodAttr?.Method != null &&
                    httpMethodAttr.Method.Equals(contextMethod, StringComparison.OrdinalIgnoreCase))
                {
                    return endpointType;
                }
            }
        }

        return null;
    }

    private static bool RoutePatternMatches(string[] contextParts, string[] endpointParts)
    {
        if (contextParts.Length != endpointParts.Length)
            return false;

        for (int i = 0; i < contextParts.Length; i++)
        {
            var contextPart = contextParts[i].ToLower();
            var endpointPart = endpointParts[i].ToLower();

            // If endpoint part is a parameter (contains { or }), it matches any context part
            if (endpointPart.Contains('{') || endpointPart.Contains('}'))
                continue;

            // Otherwise, parts must match exactly
            if (contextPart != endpointPart)
                return false;
        }

        return true;
    }

    private static string NormalizeRoute(string route)
    {
        // Remove leading slash and convert to lowercase
        var normalized = route.TrimStart('/').ToLower();
        
        // Keep route parameters for better matching
        return normalized;
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
        var handleMethod = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
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

    private static bool IsCommandRequest(Type? requestType)
    {
        if (requestType == null) return false;

        return requestType.GetInterfaces().Any(i =>
            i == typeof(ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));
    }

    private static bool IsQueryRequest(Type? requestType)
    {
        if (requestType == null) return false;

        return requestType.GetInterfaces().Any(i =>
            i == typeof(IQueryRequest) ||
            (i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(IQueryRequest<>) ||
                i.GetGenericTypeDefinition() == typeof(IQueryCollectionRequest<>))));
    }

    private static void GenerateOperationTags(OpenApiOperation operation, Type endpointType)
    {
        var tags = new List<string>();

        // Use endpoint namespace structure: sample.API.Endpoints.{Feature}.{OperationType}
        var namespaceParts = endpointType.Namespace?.Split('.') ?? [];
        var endpointsIndex = Array.FindIndex(namespaceParts, part => 
            part.Equals("Endpoints", StringComparison.OrdinalIgnoreCase));

        if (endpointsIndex >= 0 && endpointsIndex + 1 < namespaceParts.Length)
        {
            var featureName = namespaceParts[endpointsIndex + 1]; // Todo
            
            if (endpointsIndex + 2 < namespaceParts.Length)
            {
                var operationType = namespaceParts[endpointsIndex + 2]; // Commands or Queries
                
                // Only add tags for Commands or Queries
                if (operationType.Contains("Command", StringComparison.OrdinalIgnoreCase) ||
                    operationType.Contains("Quer", StringComparison.OrdinalIgnoreCase))
                {
                    tags.Add($"{featureName} {operationType}");
                }
            }
        }

        // Apply tags to operation
        if (tags.Count > 0)
        {
            operation.Tags = [.. tags.Select(tag => new OpenApiTag { Name = tag })];
        }
    }

    private static void ProcessRequestParameters(OpenApiOperation operation, Type requestType, bool isCommandRequest, bool isQueryRequest)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Initialize parameters collection
        operation.Parameters ??= [];

        // Always generate at least some parameters for testing
        var hasAnyParameters = false;

        // Check if this is a form data request (has IFormFile or FromForm attributes)
        var hasFormData = properties.Any(p => 
            p.PropertyType == typeof(IFormFile) || 
            p.PropertyType == typeof(IFormFile[]) ||
            p.PropertyType == typeof(List<IFormFile>) ||
            p.GetCustomAttribute<FromFormAttribute>() != null);

        var bodyProperties = new List<PropertyInfo>();
        var formProperties = new List<PropertyInfo>();

        foreach (var property in properties)
        {
            // Skip service injection properties
            if (property.GetCustomAttribute<FromServicesAttribute>() != null)
                continue;

            var parameterLocation = DetermineParameterLocation(property, isCommandRequest, isQueryRequest, hasFormData);

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
                            p.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase));
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
        else if (bodyProperties.Any())
        {
            GenerateRequestBodyForCommand(operation, requestType, bodyProperties);
        }
        else if (isCommandRequest && !hasAnyParameters)
        {
            // For command requests without explicit parameters, create a request body with all simple properties as query params
            // and complex properties in body
            var allSimpleProperties = properties.Where(p => 
                p.GetCustomAttribute<FromServicesAttribute>() == null && 
                IsSimpleType(p.PropertyType)).ToList();
                
            foreach (var prop in allSimpleProperties)
            {
                var param = CreateParameterFromProperty(prop, Attributes.ParameterLocation.Query);
                if (param != null)
                {
                    operation.Parameters.Add(param);
                    hasAnyParameters = true;
                }
            }
        }

        // If still no parameters, generate some basic ones for testing
        if (!hasAnyParameters && operation.Parameters.Count == 0)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "id",
                Required = false,
                Description = "Resource identifier",
                In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
            });
        }
    }

    private static MinimalAPI.Attributes.ParameterLocation DetermineParameterLocation(PropertyInfo property, bool isCommandRequest, bool isQueryRequest, bool hasFormData)
    {
        // Check for explicit binding attributes first
        if (property.GetCustomAttribute<FromRouteAttribute>() != null)
            return Attributes.ParameterLocation.Path;
        if (property.GetCustomAttribute<FromQueryAttribute>() != null)
            return Attributes.ParameterLocation.Query;
        if (property.GetCustomAttribute<FromHeaderAttribute>() != null)
            return Attributes.ParameterLocation.Header;
        if (property.GetCustomAttribute<FromBodyAttribute>() != null)
            return Attributes.ParameterLocation.Body;
        if (property.GetCustomAttribute<FromFormAttribute>() != null)
            return Attributes.ParameterLocation.Form;

        // Handle IFormFile types - these should always be form parameters
        if (property.PropertyType == typeof(IFormFile) || 
            property.PropertyType == typeof(IFormFile[]) ||
            property.PropertyType == typeof(List<IFormFile>))
        {
            return Attributes.ParameterLocation.Form;
        }

        // For all cases, default simple properties to query parameters
        // This ensures we always generate some parameters for testing
        if (IsSimpleType(property.PropertyType))
        {
            return Attributes.ParameterLocation.Query;
        }

        // Smart defaults based on request type and property characteristics
        if (isQueryRequest)
        {
            // Query requests: all properties should be query parameters unless explicitly specified
            return Attributes.ParameterLocation.Query;
        }
        
        if (isCommandRequest)
        {
            // For command requests, check if we have form data
            if (hasFormData)
            {
                // If there's form data, non-explicit properties go to form
                return Attributes.ParameterLocation.Form;
            }
            else
            {
                // Complex types go to body for commands
                return Attributes.ParameterLocation.Body;
            }
        }

        // Default fallback - always generate query parameters
        return Attributes.ParameterLocation.Query;
    }

    private static bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(Guid) ||
               underlyingType.IsEnum;
    }

    private static OpenApiParameter? CreateParameterFromProperty(PropertyInfo property, MinimalAPI.Attributes.ParameterLocation location)
    {
        if (location == Attributes.ParameterLocation.Body)
            return null;

        var parameter = new OpenApiParameter
        {
            Name = GetParameterName(property),
            Required = IsRequiredParameter(property),
            Description = GetParameterDescription(property),
            In = MapParameterLocation(location),
            Schema = CreateSchemaForType(property.PropertyType)
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
        var routeAttr = property.GetCustomAttribute<FromRouteAttribute>();
        if (routeAttr != null && !string.IsNullOrEmpty(routeAttr.Name))
            return routeAttr.Name;

        var queryAttr = property.GetCustomAttribute<FromQueryAttribute>();
        if (queryAttr != null && !string.IsNullOrEmpty(queryAttr.Name))
            return queryAttr.Name;

        var headerAttr = property.GetCustomAttribute<FromHeaderAttribute>();
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

    private static string GetParameterDescription(PropertyInfo property) => $"{property.Name} parameter";

    private static Microsoft.OpenApi.Any.IOpenApiAny? GetParameterExample(PropertyInfo property)
    {
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (propertyType == typeof(string))
            return new Microsoft.OpenApi.Any.OpenApiString("example");
        if (propertyType == typeof(int))
            return new Microsoft.OpenApi.Any.OpenApiInteger(1);
        if (propertyType == typeof(bool))
            return new Microsoft.OpenApi.Any.OpenApiBoolean(true);

        return null;
    }

    private static Microsoft.OpenApi.Models.ParameterLocation? MapParameterLocation(MinimalAPI.Attributes.ParameterLocation location) => location switch
    {
        Attributes.ParameterLocation.Query => Microsoft.OpenApi.Models.ParameterLocation.Query,
        Attributes.ParameterLocation.Path => Microsoft.OpenApi.Models.ParameterLocation.Path,
        Attributes.ParameterLocation.Header => Microsoft.OpenApi.Models.ParameterLocation.Header,
        Attributes.ParameterLocation.Cookie => Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Attributes.ParameterLocation.Form => null, // Form parameters are handled in request body
        Attributes.ParameterLocation.Body => null, // Body parameters are handled in request body
        _ => Microsoft.OpenApi.Models.ParameterLocation.Query
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
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
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
        else if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>))
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

    private static void GenerateRequestBodyForCommand(OpenApiOperation operation, Type requestType, List<PropertyInfo>? bodyProperties = null)
    {
        // Only generate request body if we have specific body properties or no parameters at all
        var properties = bodyProperties ?? [];
        
        // If no specific body properties provided, check if we should include all properties
        if (bodyProperties == null)
        {
            var allProperties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in allProperties)
            {
                if (!HasExplicitNonBodyBinding(prop) && !IsSimpleType(prop.PropertyType))
                {
                    properties.Add(prop);
                }
            }
        }

        if (properties.Count == 0) return;

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
                AdditionalPropertiesAllowed = false
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = requiredProperties.Count > 0,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = requestBodySchema
                    }
                }
            };
        }
    }

    private static void GenerateFormDataRequestBody(OpenApiOperation operation, Type requestType, List<PropertyInfo>? formProperties = null)
    {
        var properties = formProperties ?? requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
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
                    Description = $"File upload for {property.Name}"
                };
            }
            else if (property.PropertyType == typeof(IFormFile[]) || property.PropertyType == typeof(List<IFormFile>))
            {
                propertySchema = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    },
                    Description = $"Multiple file uploads for {property.Name}"
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
                AdditionalPropertiesAllowed = false
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = requiredProperties.Count > 0,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = requestBodySchema
                    }
                }
            };
        }
    }

    private static bool HasExplicitNonBodyBinding(PropertyInfo property) => property.GetCustomAttribute<FromRouteAttribute>() != null ||
               property.GetCustomAttribute<FromQueryAttribute>() != null ||
               property.GetCustomAttribute<FromHeaderAttribute>() != null ||
               property.GetCustomAttribute<FromServicesAttribute>() != null ||
               property.GetCustomAttribute<FromBodyAttribute>() != null;

    private static bool HasExplicitNonFormBinding(PropertyInfo property) => 
        property.GetCustomAttribute<FromRouteAttribute>() != null ||
        property.GetCustomAttribute<FromQueryAttribute>() != null ||
        property.GetCustomAttribute<FromHeaderAttribute>() != null ||
        property.GetCustomAttribute<FromServicesAttribute>() != null ||
        property.GetCustomAttribute<FromBodyAttribute>() != null;
}
