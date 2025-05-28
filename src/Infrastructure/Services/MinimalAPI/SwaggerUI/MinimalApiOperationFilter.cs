using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using MinimalAPI.Handlers;
using MinimalAPI.Handlers.Command;
using Microsoft.AspNetCore.Http;

namespace MinimalAPI.SwaggerUI;

public class MinimalApiOperationFilter(SwaggerUIOptions options) : IOperationFilter
{
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
    }

    private Type? FindEndpointType(OperationFilterContext context)
    {
        if (options.EndpointAssemblies == null || options.EndpointAssemblies.Length == 0)
            return null;

        var endpointTypes = options.EndpointAssemblies
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

    private static string NormalizeRoute(string route) => route.TrimStart('/').ToLower();

    private static Type? GetRequestType(Type endpointType)
    {
        var baseType = endpointType.BaseType;
        while (baseType != null && !baseType.IsGenericType)
        {
            baseType = baseType.BaseType;
        }

        return baseType?.IsGenericType == true ? baseType.GetGenericArguments().FirstOrDefault() : null;
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

        if (tags.Count != 0)
        {
            operation.Tags = [.. tags.Select(tag => new OpenApiTag { Name = tag })];
        }
    }

    private static void ProcessRequestParameters(OpenApiOperation operation, Type requestType, bool isCommandRequest, bool isQueryRequest)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Clear existing parameters to rebuild them properly
        operation.Parameters ??= [];

        // Check if this is a form data request (has IFormFile or FromForm attributes)
        var hasFormData = properties.Any(p => 
            p.PropertyType == typeof(IFormFile) || 
            p.PropertyType == typeof(IFormFile[]) ||
            p.PropertyType == typeof(List<IFormFile>) ||
            p.GetCustomAttribute<FromFormAttribute>() != null);

        foreach (var property in properties)
        {
            // Skip service injection properties
            if (property.GetCustomAttribute<FromServicesAttribute>() != null)
                continue;

            var parameterLocation = DetermineParameterLocation(property, isCommandRequest, isQueryRequest, hasFormData);

            // For form data and body parameters, they're handled in request body, not as parameters
            if (parameterLocation == Attributes.ParameterLocation.Form || 
                parameterLocation == Attributes.ParameterLocation.Body)
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
            if (hasFormData)
            {
                GenerateFormDataRequestBody(operation, requestType);
            }
            else
            {
                GenerateRequestBodyForCommand(operation, requestType);
            }
        }
    }

    private static MinimalAPI.Attributes.ParameterLocation DetermineParameterLocation(PropertyInfo property, bool isCommandRequest, bool isQueryRequest, bool hasFormData)
    {
        // Check for explicit binding attributes
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

        // Handle IFormFile types - these should always be form parameters, not body
        if (property.PropertyType == typeof(IFormFile) || 
            property.PropertyType == typeof(IFormFile[]) ||
            property.PropertyType == typeof(List<IFormFile>))
        {
            return Attributes.ParameterLocation.Form;
        }

        // Auto-detection for common route parameters
        var propertyName = property.Name.ToLower();
        if (propertyName == "id" || propertyName.EndsWith("id"))
        {
            return Attributes.ParameterLocation.Path;
        }

        // Smart defaults based on request type
        if (isCommandRequest)
        {
            return hasFormData ? Attributes.ParameterLocation.Form : Attributes.ParameterLocation.Body;
        }
        else if (isQueryRequest)
        {
            return Attributes.ParameterLocation.Query;
        }

        return Attributes.ParameterLocation.Query;
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
        var isNullable = !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null;
        return !isNullable;
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

    private static void GenerateRequestBodyForCommand(OpenApiOperation operation, Type requestType)
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

    private static void GenerateFormDataRequestBody(OpenApiOperation operation, Type requestType)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var formProperties = new Dictionary<string, OpenApiSchema>();
        var requiredProperties = new HashSet<string>();

        foreach (var property in properties)
        {
            // Skip properties with explicit non-form bindings
            if (HasExplicitNonFormBinding(property))
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

            formProperties[property.Name] = propertySchema;

            if (IsRequiredParameter(property))
            {
                requiredProperties.Add(property.Name);
            }
        }

        if (formProperties.Count > 0)
        {
            var requestBodySchema = new OpenApiSchema
            {
                Type = "object",
                Properties = formProperties,
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
