using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using System.Reflection;
using System.Text.Json;

namespace MinimalAPI.OpenApi;

public class MinimalApiDocumentTransformer(OpenApiOptions options) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        // Set document info
        document.Info.Title = options.Title;
        document.Info.Version = options.Version;
        if (!string.IsNullOrEmpty(options.Description))
        {
            document.Info.Description = options.Description;
        }

        // Process endpoints for custom documentation
        ProcessEndpoints(document);

        return Task.CompletedTask;
    }

    private void ProcessEndpoints(OpenApiDocument document)
    {
        if (options.EndpointAssemblies == null || options.EndpointAssemblies.Length == 0)
            return;

        // Find all endpoint classes
        var endpointTypes = options.EndpointAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(
                t =>
                    !t.IsAbstract
                    && !t.IsInterface
                    && t.IsAssignableTo(typeof(EndpointAbstractBase))
            )
            .ToList();

        foreach (var endpointType in endpointTypes)
        {
            ProcessEndpointType(document, endpointType);
        }
    }

    private static void ProcessEndpointType(OpenApiDocument document, Type endpointType)
    {
        var methods = endpointType
            .GetMethods()
            .Where(m => m.GetCustomAttributes().Any(a => a is HttpMethodAttribute))
            .ToList();

        foreach (var method in methods)
        {
            if (method.GetCustomAttributes().FirstOrDefault(a => a is HttpMethodAttribute) is not HttpMethodAttribute httpMethodAttr || string.IsNullOrEmpty(httpMethodAttr.Route))
                continue;

            var route = httpMethodAttr.Route;
            var httpMethod = httpMethodAttr.Method.ToLower();

            // Find the corresponding operation in the document
            if (TryFindOperation(document, route, httpMethod, out var pathItem, out var operation))
            {
                EnhanceOperation(operation: operation ?? new(),
                                 method,
                                 endpointType);
            }
        }
    }

    private static bool TryFindOperation(
        OpenApiDocument document,
        string route,
        string method,
        out OpenApiPathItem? pathItem,
        out OpenApiOperation? operation
    )
    {
        pathItem = null;
        operation = null;

        // Normalize route for matching
        var normalizedRoute = NormalizeRoute(route);

        foreach (var path in document.Paths)
        {
            if (NormalizeRoute(path.Key) == normalizedRoute)
            {
                pathItem = path.Value;
                return pathItem.Operations.TryGetValue(GetOperationType(method), out operation);
            }
        }

        return false;
    }

    private static string NormalizeRoute(string route) => route.TrimStart('/').Replace("{", "{").Replace("}", "}");

    private static OperationType GetOperationType(string method) => method.ToLower() switch
    {
        "get" => OperationType.Get,
        "post" => OperationType.Post,
        "put" => OperationType.Put,
        "delete" => OperationType.Delete,
        "patch" => OperationType.Patch,
        "options" => OperationType.Options,
        "head" => OperationType.Head,
        "trace" => OperationType.Trace,
        _ => OperationType.Get
    };

    private static void EnhanceOperation(OpenApiOperation operation, MethodInfo method, Type endpointType)
    {
        // Determine request type for smart defaults
        var requestType = GetRequestType(endpointType);
        var isCommandRequest = IsCommandRequest(requestType);
        var isQueryRequest = IsQueryRequest(requestType);

        // Auto-generate feature-based tags
        var featureTags = GenerateFeatureBasedTags(endpointType, method);

        // Process OpenApiSummary attribute
        var summaryAttr =
            method.GetCustomAttribute<OpenApiSummaryAttribute>()
            ?? endpointType.GetCustomAttribute<OpenApiSummaryAttribute>();

        if (summaryAttr != null)
        {
            operation.Summary = summaryAttr.Summary;
            if (!string.IsNullOrEmpty(summaryAttr.Description))
            {
                operation.Description = summaryAttr.Description;
            }

            // Combine custom tags with feature-based tags
            var allTags = new List<string>();
            if (summaryAttr.Tags != null && summaryAttr.Tags.Length > 0)
            {
                allTags.AddRange(summaryAttr.Tags);
            }
            allTags.AddRange(featureTags);

            operation.Tags = [.. allTags.Distinct().Select(tag => new OpenApiTag { Name = tag })];
        }
        else
        {
            // Use feature-based tags if no custom summary
            operation.Tags = [.. featureTags.Select(tag => new OpenApiTag { Name = tag })];
        }

        // Process OpenApiParameter attributes
        var parameterAttrs = method
            .GetCustomAttributes<OpenApiParameterAttribute>()
            .Concat(endpointType.GetCustomAttributes<OpenApiParameterAttribute>())
            .ToList();

        // Handle body parameters for command requests
        if (isCommandRequest && requestType != null)
        {
            GenerateRequestBodySchema(operation, requestType, parameterAttrs);
        }

        foreach (var paramAttr in parameterAttrs)
        {
            var effectiveLocation = DetermineParameterLocation(
                paramAttr.Location,
                isCommandRequest,
                isQueryRequest,
                paramAttr.Name
            );

            // Skip body parameters as they're handled in request body
            if (effectiveLocation == Attributes.ParameterLocation.Body)
                continue;

            var parameter = new OpenApiParameter
            {
                Name = paramAttr.Name,
                Required = paramAttr.Required,
                Description = paramAttr.Description,
                In = MapParameterLocation(effectiveLocation),
                Schema = CreateSchemaForType(paramAttr.Type),
                Example =
                    paramAttr.Example != null
                        ? CreateOpenApiAnyFromObject(paramAttr.Example)
                        : null
            };

            if (!string.IsNullOrEmpty(paramAttr.Format))
            {
                parameter.Schema.Format = paramAttr.Format;
            }

            operation.Parameters ??= [];
            operation.Parameters.Add(parameter);
        }

        // Auto-generate parameters from request type if no explicit attributes
        if (parameterAttrs.Count == 0 && requestType != null)
        {
            GenerateParametersFromRequestType(
                operation,
                requestType,
                isCommandRequest,
                isQueryRequest
            );
        }

        // Process OpenApiResponse attributes
        var responseAttrs = method
            .GetCustomAttributes<OpenApiResponseAttribute>()
            .Concat(endpointType.GetCustomAttributes<OpenApiResponseAttribute>())
            .ToList();

        foreach (var responseAttr in responseAttrs)
        {
            var response = new OpenApiResponse
            {
                Description = responseAttr.Description ?? $"Response {responseAttr.StatusCode}"
            };

            if (responseAttr.ResponseType != null)
            {
                response.Content = new Dictionary<string, OpenApiMediaType>
                {
                    [responseAttr.ContentType ?? "application/json"] = new OpenApiMediaType
                    {
                        Schema = CreateSchemaForType(responseAttr.ResponseType)
                    }
                };
            }

            operation.Responses[responseAttr.StatusCode.ToString()] = response;
        }
    }

    private static Type? GetRequestType(Type endpointType)
    {
        // Find the base type with generic arguments (e.g., SingleEndpointBase<TRequest, TResponse>)
        var baseType = endpointType.BaseType;
        while (baseType != null && !baseType.IsGenericType)
        {
            baseType = baseType.BaseType;
        }

        if (baseType?.IsGenericType == true)
        {
            var genericArgs = baseType.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                return genericArgs[0]; // First generic argument is typically the request type
            }
        }

        return null;
    }

    private static bool IsCommandRequest(Type? requestType)
    {
        if (requestType == null)
            return false;

        return requestType
            .GetInterfaces()
            .Any(
                i =>
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
            .Any(
                i =>
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

    private static MinimalAPI.Attributes.ParameterLocation DetermineParameterLocation(
        MinimalAPI.Attributes.ParameterLocation specifiedLocation,
        bool isCommandRequest,
        bool isQueryRequest,
        string parameterName
    )
    {
        // If explicitly specified, use that
        if (specifiedLocation != Attributes.ParameterLocation.Auto)
        {
            return specifiedLocation;
        }

        // Check if it's a route parameter (contains parameter name in curly braces)
        if (parameterName.Equals("id", StringComparison.OrdinalIgnoreCase))
        {
            return Attributes.ParameterLocation.Path;
        }

        // Apply smart defaults based on request type
        if (isCommandRequest)
        {
            return Attributes.ParameterLocation.Body;
        }
        else if (isQueryRequest)
        {
            return Attributes.ParameterLocation.Query;
        }

        // Default fallback
        return Attributes.ParameterLocation.Query;
    }

    private static List<string> GenerateFeatureBasedTags(Type endpointType, MethodInfo method)
    {
        var tags = new List<string>();

        // Extract feature from namespace
        var namespaceParts = endpointType.Namespace?.Split('.') ?? [];

        // Look for feature patterns like "Features.Todo.Commands" or "Features.Todo.Queries"
        var featureIndex = Array.FindIndex(namespaceParts, part =>
            part.Equals("Features", StringComparison.OrdinalIgnoreCase));

        if (featureIndex >= 0 && featureIndex + 1 < namespaceParts.Length)
        {
            var featureName = namespaceParts[featureIndex + 1]; // e.g., "Todo"
            tags.Add(featureName);

            // Add operation type if available
            if (featureIndex + 2 < namespaceParts.Length)
            {
                var operationType = namespaceParts[featureIndex + 2]; // e.g., "Commands", "Queries"
                tags.Add($"{featureName} {operationType}");
            }
        }
        else
        {
            // Fallback: extract from endpoint class name
            var className = endpointType.Name;
            if (className.EndsWith("Endpoint"))
            {
                className = className[..^8]; // Remove "Endpoint" suffix
            }

        }

        return tags;
    }

    private static OpenApiSchema CreateSchemaForType(Type type)
    {
        var schema = new OpenApiSchema();

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Handle IFormFile types first
        if (underlyingType == typeof(IFormFile))
        {
            schema.Type = "string";
            schema.Format = "binary";
            return schema;
        }
        else if (underlyingType == typeof(IFormFile[]) || 
                 (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>) && 
                  underlyingType.GetGenericArguments()[0] == typeof(IFormFile)))
        {
            schema.Type = "array";
            schema.Items = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };
            return schema;
        }

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
        else if (underlyingType == typeof(DateTimeOffset))
        {
            schema.Type = "string";
            schema.Format = "date-time";
        }
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            schema.Type = "number";
            if (underlyingType == typeof(float))
                schema.Format = "float";
            else if (underlyingType == typeof(double))
                schema.Format = "double";
        }
        else if (underlyingType == typeof(Guid))
        {
            schema.Type = "string";
            schema.Format = "uuid";
        }
        else if (underlyingType.IsEnum)
        {
            schema.Type = "string";
            schema.Enum = [];
            foreach (var enumName in Enum.GetNames(underlyingType))
            {
                schema.Enum.Add(new OpenApiString(enumName));
            }
        }
        else if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>))
        {
            schema.Type = "array";
            var itemType = underlyingType.GetGenericArguments()[0];
            schema.Items = CreateSchemaForType(itemType);
        }
        else if (underlyingType.IsArray)
        {
            schema.Type = "array";
            var elementType = underlyingType.GetElementType();
            if (elementType != null)
            {
                schema.Items = CreateSchemaForType(elementType);
            }
        }
        else
        {
            schema.Type = "object";
            schema.Properties = new Dictionary<string, OpenApiSchema>();
            schema.AdditionalPropertiesAllowed = true;
        }

        // Set nullable if the original type was nullable
        if (type != underlyingType)
        {
            schema.Nullable = true;
        }

        return schema;
    }

    private static void GenerateRequestBodySchema(OpenApiOperation operation, Type requestType, List<OpenApiParameterAttribute> parameterAttrs)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var bodyProperties = new Dictionary<string, OpenApiSchema>();
        var requiredProperties = new HashSet<string>();

        // Check if this is a form data request
        var hasFormData = properties.Any(p => 
            p.PropertyType == typeof(IFormFile) || 
            p.PropertyType == typeof(IFormFile[]) ||
            (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && 
             p.PropertyType.GetGenericArguments()[0] == typeof(IFormFile)) ||
            p.GetCustomAttribute<FromFormAttribute>() != null);

        var contentType = hasFormData ? "multipart/form-data" : "application/json";

        foreach (var property in properties)
        {
            // Skip properties that are explicitly bound to other locations
            if (property.GetCustomAttribute<FromServicesAttribute>() != null ||
                property.GetCustomAttribute<FromRouteAttribute>() != null ||
                property.GetCustomAttribute<FromQueryAttribute>() != null ||
                property.GetCustomAttribute<FromHeaderAttribute>() != null)
                continue;

            // Check if there's an explicit FromBody/FromForm attribute or if it's a command (default to body/form)
            var fromBodyAttr = property.GetCustomAttribute<FromBodyAttribute>();
            var fromFormAttr = property.GetCustomAttribute<FromFormAttribute>();
            var isFileProperty = property.PropertyType == typeof(IFormFile) || 
                                property.PropertyType == typeof(IFormFile[]) ||
                                (property.PropertyType.IsGenericType && 
                                 property.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && 
                                 property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile));

            // Include property if it has form/body attributes, is a file property, or has no explicit binding
            if (fromBodyAttr != null || fromFormAttr != null || isFileProperty || !HasExplicitBindingAttribute(property))
            {
                var propertySchema = CreateSchemaForType(property.PropertyType);
                
                // Add description based on property type
                if (isFileProperty)
                {
                    if (property.PropertyType == typeof(IFormFile))
                    {
                        propertySchema.Description = $"File upload for {property.Name}";
                    }
                    else
                    {
                        propertySchema.Description = $"Multiple file uploads for {property.Name}";
                    }
                }
                else
                {
                    propertySchema.Description = $"The {property.Name} property";
                }

                bodyProperties[property.Name] = propertySchema;

                // Check if property is required (non-nullable value types or properties with Required attribute)
                if (!IsNullableType(property.PropertyType) || 
                    property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null)
                {
                    requiredProperties.Add(property.Name);
                }
            }
        }

        if (bodyProperties.Count > 0)
        {
            var requestBodySchema = new OpenApiSchema
            {
                Type = "object",
                Properties = bodyProperties,
                Required = requiredProperties,
                AdditionalPropertiesAllowed = false
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = requiredProperties.Count > 0,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [contentType] = new OpenApiMediaType
                    {
                        Schema = requestBodySchema
                    }
                }
            };
        }
    }

    private static bool HasExplicitBindingAttribute(PropertyInfo property) => 
        property.GetCustomAttribute<FromRouteAttribute>() != null ||
        property.GetCustomAttribute<FromQueryAttribute>() != null ||
        property.GetCustomAttribute<FromHeaderAttribute>() != null ||
        property.GetCustomAttribute<FromFormAttribute>() != null ||
        property.GetCustomAttribute<FromServicesAttribute>() != null ||
        property.GetCustomAttribute<FromBodyAttribute>() != null;

    private static void GenerateParametersFromRequestType(
        OpenApiOperation operation,
        Type requestType,
        bool isCommandRequest,
        bool isQueryRequest
    )
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Check if this is a form data request
        var hasFormData = properties.Any(p => 
            p.PropertyType == typeof(IFormFile) || 
            p.PropertyType == typeof(IFormFile[]) ||
            (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && 
             p.PropertyType.GetGenericArguments()[0] == typeof(IFormFile)) ||
            p.GetCustomAttribute<FromFormAttribute>() != null);

        foreach (var property in properties)
        {
            // Skip properties that have FromServices attribute
            if (property.GetCustomAttribute<FromServicesAttribute>() != null)
                continue;

            var location = DetermineParameterLocation(
                Attributes.ParameterLocation.Auto,
                isCommandRequest,
                isQueryRequest,
                property.Name
            );

            // Check for explicit binding attributes
            if (property.GetCustomAttribute<FromRouteAttribute>() != null)
                location = Attributes.ParameterLocation.Path;
            else if (property.GetCustomAttribute<FromQueryAttribute>() != null)
                location = Attributes.ParameterLocation.Query;
            else if (property.GetCustomAttribute<FromBodyAttribute>() != null)
                location = Attributes.ParameterLocation.Body;
            else if (property.GetCustomAttribute<FromHeaderAttribute>() != null)
                location = Attributes.ParameterLocation.Header;
            else if (property.GetCustomAttribute<FromFormAttribute>() != null)
                location = Attributes.ParameterLocation.Form;

            // Handle IFormFile types - these should always be form parameters
            if (property.PropertyType == typeof(IFormFile) || 
                property.PropertyType == typeof(IFormFile[]) ||
                (property.PropertyType.IsGenericType && 
                 property.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && 
                 property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile)))
            {
                location = Attributes.ParameterLocation.Form;
            }

            // Skip body and form parameters for commands as they're handled in request body
            if ((location == Attributes.ParameterLocation.Body || location == Attributes.ParameterLocation.Form) && isCommandRequest)
                continue;

            var parameter = new OpenApiParameter
            {
                Name = property.Name,
                Required = !IsNullableType(property.PropertyType) || 
                          property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null,
                Description = $"{property.Name} parameter",
                In = MapParameterLocation(location),
                Schema = CreateSchemaForType(property.PropertyType)
            };

            // Only add if it's not a form/body parameter
            if (parameter.In != null)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(parameter);
            }
        }
    }

    private static bool IsNullableType(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

    private static Microsoft.OpenApi.Models.ParameterLocation? MapParameterLocation(
        MinimalAPI.Attributes.ParameterLocation location
    ) => location switch
    {
        Attributes.ParameterLocation.Query
            => Microsoft.OpenApi.Models.ParameterLocation.Query,
        Attributes.ParameterLocation.Path
            => Microsoft.OpenApi.Models.ParameterLocation.Path,
        Attributes.ParameterLocation.Header
            => Microsoft.OpenApi.Models.ParameterLocation.Header,
        Attributes.ParameterLocation.Cookie
            => Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Attributes.ParameterLocation.Body => null, // Body parameters are handled differently in OpenAPI
        Attributes.ParameterLocation.Form => null, // Form parameters are handled in request body
        _ => Microsoft.OpenApi.Models.ParameterLocation.Query
    };

    private static IOpenApiAny CreateOpenApiAnyFromObject(object value)
    {
        if (value == null)
            return new OpenApiNull();

        return value switch
        {
            string stringValue => new OpenApiString(stringValue),
            int intValue => new OpenApiInteger(intValue),
            long longValue => new OpenApiLong(longValue),
            bool boolValue => new OpenApiBoolean(boolValue),
            double doubleValue => new OpenApiDouble(doubleValue),
            float floatValue => new OpenApiFloat(floatValue),
            decimal decimalValue => new OpenApiDouble((double)decimalValue),
            DateTime dateTimeValue => new OpenApiString(dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
            DateTimeOffset dateTimeOffsetValue => new OpenApiString(dateTimeOffsetValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
            Guid guidValue => new OpenApiString(guidValue.ToString()),
            _ => new OpenApiString(JsonSerializer.Serialize(value))
        };
    }
}
