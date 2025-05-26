using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using Microsoft.OpenApi.Any;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using System.Text.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MinimalAPI.OpenApi;

public class MinimalApiDocumentTransformer : IOpenApiDocumentTransformer
{
    private readonly OpenApiOptions _options;

    public MinimalApiDocumentTransformer(OpenApiOptions options)
    {
        _options = options;
    }

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        // Set document info
        document.Info.Title = _options.Title;
        document.Info.Version = _options.Version;
        if (!string.IsNullOrEmpty(_options.Description))
        {
            document.Info.Description = _options.Description;
        }

        // Process endpoints for custom documentation
        ProcessEndpoints(document);

        return Task.CompletedTask;
    }

    private void ProcessEndpoints(OpenApiDocument document)
    {
        if (_options.EndpointAssemblies == null || _options.EndpointAssemblies.Length == 0)
            return;

        // Find all endpoint classes
        var endpointTypes = _options.EndpointAssemblies
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

    private void ProcessEndpointType(OpenApiDocument document, Type endpointType)
    {
        var methods = endpointType
            .GetMethods()
            .Where(m => m.GetCustomAttributes().Any(a => a is HttpMethodAttribute))
            .ToList();

        foreach (var method in methods)
        {
            var httpMethodAttr =
                method.GetCustomAttributes().FirstOrDefault(a => a is HttpMethodAttribute)
                as HttpMethodAttribute;

            if (httpMethodAttr == null || string.IsNullOrEmpty(httpMethodAttr.Route))
                continue;

            var route = httpMethodAttr.Route;
            var httpMethod = httpMethodAttr.Method.ToLower();

            // Find the corresponding operation in the document
            if (TryFindOperation(document, route, httpMethod, out var pathItem, out var operation))
            {
                EnhanceOperation(operation, method, endpointType);
            }
        }
    }

    private bool TryFindOperation(
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

    private string NormalizeRoute(string route)
    {
        return route.TrimStart('/').Replace("{", "{").Replace("}", "}");
    }

    private OperationType GetOperationType(string method)
    {
        return method.ToLower() switch
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
    }

    private void EnhanceOperation(OpenApiOperation operation, MethodInfo method, Type endpointType)
    {
        // Determine request type for smart defaults
        var requestType = GetRequestType(endpointType);
        var isCommandRequest = IsCommandRequest(requestType);
        var isQueryRequest = IsQueryRequest(requestType);

        // Process EndpointSummary attribute
        var summaryAttr =
            method.GetCustomAttribute<EndpointSummaryAttribute>()
            ?? endpointType.GetCustomAttribute<EndpointSummaryAttribute>();

        if (summaryAttr != null)
        {
            operation.Summary = summaryAttr.Summary;
            if (!string.IsNullOrEmpty(summaryAttr.Description))
            {
                operation.Description = summaryAttr.Description;
            }
            if (summaryAttr.Tags != null && summaryAttr.Tags.Length > 0)
            {
                operation.Tags = summaryAttr.Tags
                    .Select(tag => new OpenApiTag { Name = tag })
                    .ToList();
            }
        }

        // Process OpenApiParameter attributes
        var parameterAttrs = method
            .GetCustomAttributes<OpenApiParameterAttribute>()
            .Concat(endpointType.GetCustomAttributes<OpenApiParameterAttribute>())
            .ToList();

        foreach (var paramAttr in parameterAttrs)
        {
            var effectiveLocation = DetermineParameterLocation(
                paramAttr.Location, 
                isCommandRequest, 
                isQueryRequest,
                paramAttr.Name
            );

            var parameter = new OpenApiParameter
            {
                Name = paramAttr.Name,
                Required = paramAttr.Required,
                Description = paramAttr.Description,
                In = MapParameterLocation(effectiveLocation),
                Schema = CreateSchemaForType(paramAttr.Type),
                Example =
                    paramAttr.Example != null
                        ? OpenApiAnyFactory.CreateFromJson(
                            System.Text.Json.JsonSerializer.Serialize(paramAttr.Example)
                        )
                        : null
            };

            if (!string.IsNullOrEmpty(paramAttr.Format))
            {
                parameter.Schema.Format = paramAttr.Format;
            }

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(parameter);
        }

        // Auto-generate parameters from request type if no explicit attributes
        if (!parameterAttrs.Any() && requestType != null)
        {
            GenerateParametersFromRequestType(operation, requestType, isCommandRequest, isQueryRequest);
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

    private Type? GetRequestType(Type endpointType)
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

    private bool IsCommandRequest(Type? requestType)
    {
        if (requestType == null) return false;

        return requestType.GetInterfaces().Any(i =>
            i == typeof(ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
        );
    }

    private bool IsQueryRequest(Type? requestType)
    {
        if (requestType == null) return false;

        return requestType.GetInterfaces().Any(i =>
            i == typeof(IQueryRequest) ||
            (i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(IQueryRequest<>) ||
                i.GetGenericTypeDefinition() == typeof(IQueryCollectionRequest<>)
            ))
        );
    }

    private MinimalAPI.Attributes.ParameterLocation DetermineParameterLocation(
        MinimalAPI.Attributes.ParameterLocation specifiedLocation,
        bool isCommandRequest,
        bool isQueryRequest,
        string parameterName)
    {
        // If explicitly specified, use that
        if (specifiedLocation != MinimalAPI.Attributes.ParameterLocation.Auto)
        {
            return specifiedLocation;
        }

        // Check if it's a route parameter (contains parameter name in curly braces)
        if (parameterName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
            parameterName.Equals("todoId", StringComparison.OrdinalIgnoreCase))
        {
            return MinimalAPI.Attributes.ParameterLocation.Path;
        }

        // Apply smart defaults based on request type
        if (isCommandRequest)
        {
            return MinimalAPI.Attributes.ParameterLocation.Body;
        }
        else if (isQueryRequest)
        {
            return MinimalAPI.Attributes.ParameterLocation.Query;
        }

        // Default fallback
        return MinimalAPI.Attributes.ParameterLocation.Query;
    }

    private void GenerateParametersFromRequestType(
        OpenApiOperation operation, 
        Type requestType, 
        bool isCommandRequest, 
        bool isQueryRequest)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip properties that have FromServices attribute
            if (property.GetCustomAttribute<FromServicesAttribute>() != null)
                continue;

            var location = DetermineParameterLocation(
                MinimalAPI.Attributes.ParameterLocation.Auto,
                isCommandRequest,
                isQueryRequest,
                property.Name
            );

            // Check for explicit binding attributes
            if (property.GetCustomAttribute<FromRouteAttribute>() != null)
                location = MinimalAPI.Attributes.ParameterLocation.Path;
            else if (property.GetCustomAttribute<FromQueryAttribute>() != null)
                location = MinimalAPI.Attributes.ParameterLocation.Query;
            else if (property.GetCustomAttribute<FromBodyAttribute>() != null)
                location = MinimalAPI.Attributes.ParameterLocation.Body;
            else if (property.GetCustomAttribute<FromHeaderAttribute>() != null)
                location = MinimalAPI.Attributes.ParameterLocation.Header;

            var parameter = new OpenApiParameter
            {
                Name = property.Name,
                Required = !IsNullableType(property.PropertyType),
                Description = $"{property.Name} parameter",
                In = MapParameterLocation(location),
                Schema = CreateSchemaForType(property.PropertyType)
            };

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(parameter);
        }
    }

    private bool IsNullableType(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    private Microsoft.OpenApi.Models.ParameterLocation? MapParameterLocation(MinimalAPI.Attributes.ParameterLocation location)
    {
        return location switch
        {
            MinimalAPI.Attributes.ParameterLocation.Query => Microsoft.OpenApi.Models.ParameterLocation.Query,
            MinimalAPI.Attributes.ParameterLocation.Path => Microsoft.OpenApi.Models.ParameterLocation.Path,
            MinimalAPI.Attributes.ParameterLocation.Header => Microsoft.OpenApi.Models.ParameterLocation.Header,
            MinimalAPI.Attributes.ParameterLocation.Cookie => Microsoft.OpenApi.Models.ParameterLocation.Cookie,
            MinimalAPI.Attributes.ParameterLocation.Body => null, // Body parameters are handled differently in OpenAPI
            _ => Microsoft.OpenApi.Models.ParameterLocation.Query
        };
    }

    private OpenApiSchema CreateSchemaForType(Type type)
    {
        var schema = new OpenApiSchema();

        if (type == typeof(string))
        {
            schema.Type = "string";
        }
        else if (type == typeof(int) || type == typeof(int?))
        {
            schema.Type = "integer";
            schema.Format = "int32";
        }
        else if (type == typeof(long) || type == typeof(long?))
        {
            schema.Type = "integer";
            schema.Format = "int64";
        }
        else if (type == typeof(bool) || type == typeof(bool?))
        {
            schema.Type = "boolean";
        }
        else if (type == typeof(DateTime) || type == typeof(DateTime?))
        {
            schema.Type = "string";
            schema.Format = "date-time";
        }
        else if (
            type == typeof(decimal)
            || type == typeof(decimal?)
            || type == typeof(double)
            || type == typeof(double?)
            || type == typeof(float)
            || type == typeof(float?)
        )
        {
            schema.Type = "number";
        }
        else if (type.IsEnum)
        {
            schema.Type = "string";
            schema.Enum = Enum.GetNames(type)
                .Select(
                    name => CreateOpenApiString(name)
                )
                .ToList<IOpenApiAny>();
        }
        else
        {
            schema.Type = "object";
        }

        return schema;
    }

    private IOpenApiAny CreateOpenApiAnyFromObject(object value)
    {
        if (value == null)
            return null;

        if (value is string stringValue)
            return CreateOpenApiString(stringValue);

        if (value is int intValue)
            return new OpenApiInteger(intValue);

        if (value is long longValue)
            return new OpenApiLong(longValue);

        if (value is bool boolValue)
            return new OpenApiBoolean(boolValue);

        if (value is double doubleValue)
            return new OpenApiDouble(doubleValue);

        if (value is float floatValue)
            return new OpenApiFloat(floatValue);

        // For complex objects, serialize to JSON string
        return CreateOpenApiString(JsonSerializer.Serialize(value));
    }

    private OpenApiString CreateOpenApiString(string value)
    {
        return new OpenApiString(value);
    }
}
