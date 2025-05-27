using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;

namespace MinimalAPI.SwaggerUI;

public class MinimalApiOperationFilter : IOperationFilter
{
    private readonly SwaggerUIOptions _options;

    public MinimalApiOperationFilter(SwaggerUIOptions options)
    {
        _options = options;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Find the endpoint type for this operation
        var endpointType = FindEndpointType(context);
        if (endpointType == null) return;

        var requestType = GetRequestType(endpointType);
        if (requestType == null) return;

        var isCommandRequest = IsCommandRequest(requestType);
        var isQueryRequest = IsQueryRequest(requestType);

        // Generate tags based on endpoint
        GenerateOperationTags(operation, endpointType);

        // Process parameters from request type
        ProcessRequestParameters(operation, requestType, isCommandRequest, isQueryRequest);

        // Add examples
        AddOperationExamples(operation, requestType, endpointType);
    }

    private Type? FindEndpointType(OperationFilterContext context)
    {
        if (_options.EndpointAssemblies == null || _options.EndpointAssemblies.Length == 0)
            return null;

        var endpointTypes = _options.EndpointAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(EndpointAbstractBase)))
            .ToList();

        // Try to match by action name or route
        foreach (var endpointType in endpointTypes)
        {
            var methods = endpointType.GetMethods()
                .Where(m => m.GetCustomAttributes().Any(a => a is HttpMethodAttribute))
                .ToList();

            foreach (var method in methods)
            {
                var httpMethodAttr = method.GetCustomAttributes()
                    .FirstOrDefault(a => a is HttpMethodAttribute) as HttpMethodAttribute;

                if (httpMethodAttr?.Route != null)
                {
                    var normalizedRoute = NormalizeRoute(httpMethodAttr.Route);
                    var contextRoute = NormalizeRoute(context.ApiDescription.RelativePath ?? "");
                    
                    if (normalizedRoute == contextRoute && 
                        httpMethodAttr.Method.Equals(context.ApiDescription.HttpMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        return endpointType;
                    }
                }
            }
        }

        return null;
    }

    private string NormalizeRoute(string route)
    {
        return route.TrimStart('/').ToLower();
    }

    private Type? GetRequestType(Type endpointType)
    {
        var baseType = endpointType.BaseType;
        while (baseType != null && !baseType.IsGenericType)
        {
            baseType = baseType.BaseType;
        }

        return baseType?.IsGenericType == true ? baseType.GetGenericArguments().FirstOrDefault() : null;
    }

    private bool IsCommandRequest(Type? requestType)
    {
        if (requestType == null) return false;

        return requestType.GetInterfaces().Any(i =>
            i == typeof(ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));
    }

    private bool IsQueryRequest(Type? requestType)
    {
        if (requestType == null) return false;

        return requestType.GetInterfaces().Any(i =>
            i == typeof(IQueryRequest) ||
            (i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(IQueryRequest<>) ||
                i.GetGenericTypeDefinition() == typeof(IQueryCollectionRequest<>))));
    }

    private void GenerateOperationTags(OpenApiOperation operation, Type endpointType)
    {
        var tags = new List<string>();

        // Extract from namespace
        var namespaceParts = endpointType.Namespace?.Split('.') ?? [];
        var featureIndex = Array.FindIndex(namespaceParts, part => 
            part.Equals("Features", StringComparison.OrdinalIgnoreCase));

        if (featureIndex >= 0 && featureIndex + 1 < namespaceParts.Length)
        {
            var featureName = namespaceParts[featureIndex + 1];
            tags.Add(featureName);

            if (featureIndex + 2 < namespaceParts.Length)
            {
                var operationType = namespaceParts[featureIndex + 2];
                tags.Add($"{featureName} {operationType}");
            }
        }
        else
        {
            // Fallback to class name analysis
            var className = endpointType.Name;
            if (className.EndsWith("Endpoint"))
                className = className[..^8];

            // Extract feature from patterns like "CreateTodoEndpoint"
            if (className.Contains("Todo"))
            {
                tags.Add("Todo");
                
                var operationType = className switch
                {
                    var name when name.StartsWith("Create") || name.StartsWith("Add") => "Commands",
                    var name when name.StartsWith("Update") || name.StartsWith("Edit") => "Commands", 
                    var name when name.StartsWith("Delete") || name.StartsWith("Remove") => "Commands",
                    var name when name.StartsWith("Get") || name.StartsWith("List") || name.StartsWith("Search") => "Queries",
                    _ => "Operations"
                };
                tags.Add($"Todo {operationType}");
            }
        }

        if (tags.Any())
        {
            operation.Tags = tags.Select(tag => new OpenApiTag { Name = tag }).ToList();
        }
    }

    private void ProcessRequestParameters(OpenApiOperation operation, Type requestType, bool isCommandRequest, bool isQueryRequest)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Clear existing parameters to rebuild them properly
        operation.Parameters ??= new List<OpenApiParameter>();

        foreach (var property in properties)
        {
            // Skip service injection properties
            if (property.GetCustomAttribute<FromServicesAttribute>() != null)
                continue;

            var parameterLocation = DetermineParameterLocation(property, isCommandRequest, isQueryRequest);

            // For command requests, body parameters are handled in request body, not as parameters
            if (parameterLocation == MinimalAPI.Attributes.ParameterLocation.Body && isCommandRequest)
            {
                continue;
            }

            var parameter = CreateParameterFromProperty(property, parameterLocation);
            if (parameter != null)
            {
                // Remove any existing parameter with the same name
                var existing = operation.Parameters.FirstOrDefault(p => 
                    p.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    operation.Parameters.Remove(existing);
                }
                
                operation.Parameters.Add(parameter);
            }
        }

        // Handle request body for command requests
        if (isCommandRequest)
        {
            GenerateRequestBodyForCommand(operation, requestType);
        }
    }

    private MinimalAPI.Attributes.ParameterLocation DetermineParameterLocation(PropertyInfo property, bool isCommandRequest, bool isQueryRequest)
    {
        // Check for explicit binding attributes
        if (property.GetCustomAttribute<FromRouteAttribute>() != null)
            return MinimalAPI.Attributes.ParameterLocation.Path;
        if (property.GetCustomAttribute<FromQueryAttribute>() != null)
            return MinimalAPI.Attributes.ParameterLocation.Query;
        if (property.GetCustomAttribute<FromHeaderAttribute>() != null)
            return MinimalAPI.Attributes.ParameterLocation.Header;
        if (property.GetCustomAttribute<FromBodyAttribute>() != null)
            return MinimalAPI.Attributes.ParameterLocation.Body;

        // Auto-detection for common route parameters
        var propertyName = property.Name.ToLower();
        if (propertyName == "id" || propertyName.EndsWith("id"))
        {
            return MinimalAPI.Attributes.ParameterLocation.Path;
        }

        // Smart defaults based on request type
        if (isCommandRequest)
        {
            return MinimalAPI.Attributes.ParameterLocation.Body;
        }
        else if (isQueryRequest)
        {
            return MinimalAPI.Attributes.ParameterLocation.Query;
        }

        return MinimalAPI.Attributes.ParameterLocation.Query;
    }

    private OpenApiParameter? CreateParameterFromProperty(PropertyInfo property, MinimalAPI.Attributes.ParameterLocation location)
    {
        if (location == MinimalAPI.Attributes.ParameterLocation.Body)
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

    private string GetParameterName(PropertyInfo property)
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

    private bool IsRequiredParameter(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var isNullable = !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null;
        return !isNullable;
    }

    private string GetParameterDescription(PropertyInfo property)
    {
        return $"{property.Name} parameter";
    }

    private Microsoft.OpenApi.Any.IOpenApiAny? GetParameterExample(PropertyInfo property)
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

    private Microsoft.OpenApi.Models.ParameterLocation? MapParameterLocation(MinimalAPI.Attributes.ParameterLocation location)
    {
        return location switch
        {
            MinimalAPI.Attributes.ParameterLocation.Query => Microsoft.OpenApi.Models.ParameterLocation.Query,
            MinimalAPI.Attributes.ParameterLocation.Path => Microsoft.OpenApi.Models.ParameterLocation.Path,
            MinimalAPI.Attributes.ParameterLocation.Header => Microsoft.OpenApi.Models.ParameterLocation.Header,
            MinimalAPI.Attributes.ParameterLocation.Cookie => Microsoft.OpenApi.Models.ParameterLocation.Cookie,
            _ => Microsoft.OpenApi.Models.ParameterLocation.Query
        };
    }

    private OpenApiSchema CreateSchemaForType(Type type)
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
            schema.Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>();
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

    private void GenerateRequestBodyForCommand(OpenApiOperation operation, Type requestType)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var bodyProperties = new Dictionary<string, OpenApiSchema>();
        var requiredProperties = new HashSet<string>();

        foreach (var property in properties)
        {
            // Skip properties with explicit non-body bindings
            if (HasExplicitNonBodyBinding(property))
                continue;

            var propertySchema = CreateSchemaForType(property.PropertyType);
            bodyProperties[property.Name] = propertySchema;

            if (IsRequiredParameter(property))
            {
                requiredProperties.Add(property.Name);
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
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = requestBodySchema
                    }
                }
            };
        }
    }

    private bool HasExplicitNonBodyBinding(PropertyInfo property)
    {
        return property.GetCustomAttribute<FromRouteAttribute>() != null ||
               property.GetCustomAttribute<FromQueryAttribute>() != null ||
               property.GetCustomAttribute<FromHeaderAttribute>() != null ||
               property.GetCustomAttribute<FromServicesAttribute>() != null;
    }

    private void AddOperationExamples(OpenApiOperation operation, Type requestType, Type endpointType)
    {
        // Add basic examples based on the operation type
        var className = endpointType.Name;
        
        if (className.Contains("Todo"))
        {
            AddTodoExamples(operation, requestType, className);
        }
    }

    private void AddTodoExamples(OpenApiOperation operation, Type requestType, string className)
    {
        // Add examples for Todo operations
        if (operation.RequestBody?.Content?.ContainsKey("application/json") == true)
        {
            var mediaType = operation.RequestBody.Content["application/json"];
            mediaType.Examples ??= new Dictionary<string, OpenApiExample>();

            if (className.Contains("Create"))
            {
                mediaType.Examples["create-example"] = new OpenApiExample
                {
                    Summary = "Create Todo Example",
                    Value = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["title"] = new Microsoft.OpenApi.Any.OpenApiString("Buy groceries"),
                        ["description"] = new Microsoft.OpenApi.Any.OpenApiString("Milk, bread, eggs"),
                        ["categoryId"] = new Microsoft.OpenApi.Any.OpenApiInteger(1)
                    }
                };
            }
            else if (className.Contains("Update"))
            {
                mediaType.Examples["update-example"] = new OpenApiExample
                {
                    Summary = "Update Todo Example", 
                    Value = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["title"] = new Microsoft.OpenApi.Any.OpenApiString("Updated task"),
                        ["description"] = new Microsoft.OpenApi.Any.OpenApiString("Updated description"),
                        ["isCompleted"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true)
                    }
                };
            }
        }
    }
}
