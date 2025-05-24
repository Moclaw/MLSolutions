using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using MinimalAPI.Attributes;

namespace MinimalAPI.Endpoints;

public static class EndpointBindingHelper
{
    /// <summary>
    /// Binds request data from HTTP context to request object
    /// </summary>
    public static async Task<TRequest> BindAsync<TRequest>(
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
        where TRequest : class, new()
    {
        var request = new TRequest();
        var properties = typeof(TRequest).GetProperties();

        foreach (var property in properties)
        {
            var value = await GetPropertyValueAsync(httpContext, property, cancellationToken);
            if (value != null)
            {
                property.SetValue(request, value);
            }
        }

        return request;
    }

    private static async Task<object?> GetPropertyValueAsync(
        HttpContext httpContext,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
    {
        // Check for FromRoute attribute
        var fromRouteAttr = property.GetCustomAttribute<FromRouteAttribute>();
        if (fromRouteAttr != null)
        {
            var routeKey = !string.IsNullOrEmpty(fromRouteAttr.Name) ? fromRouteAttr.Name : property.Name;
            if (httpContext.Request.RouteValues.TryGetValue(routeKey, out var routeValue))
            {
                return ConvertValue(routeValue?.ToString(), property.PropertyType);
            }
        }

        // Check for FromQuery attribute
        var fromQueryAttr = property.GetCustomAttribute<FromQueryAttribute>();
        if (fromQueryAttr != null)
        {
            var queryKey = !string.IsNullOrEmpty(fromQueryAttr.Name) ? fromQueryAttr.Name : property.Name;
            if (httpContext.Request.Query.TryGetValue(queryKey, out var queryValue))
            {
                return ConvertValue(queryValue.ToString(), property.PropertyType);
            }
        }

        // Check for FromHeader attribute
        var fromHeaderAttr = property.GetCustomAttribute<FromHeaderAttribute>();
        if (fromHeaderAttr != null)
        {
            var headerKey = !string.IsNullOrEmpty(fromHeaderAttr.Name) ? fromHeaderAttr.Name : property.Name;
            if (httpContext.Request.Headers.TryGetValue(headerKey, out var headerValue))
            {
                return ConvertValue(headerValue.ToString(), property.PropertyType);
            }
        }

        // Check for FromForm attribute
        var fromFormAttr = property.GetCustomAttribute<FromFormAttribute>();
        if (fromFormAttr != null && httpContext.Request.HasFormContentType)
        {
            var form = await httpContext.Request.ReadFormAsync(cancellationToken);
            var formKey = !string.IsNullOrEmpty(fromFormAttr.Name) ? fromFormAttr.Name : property.Name;
            if (form.TryGetValue(formKey, out var formValue))
            {
                return ConvertValue(formValue.ToString(), property.PropertyType);
            }
        }

        // Check for FromBody attribute
        var fromBodyAttr = property.GetCustomAttribute<FromBodyAttribute>();
        if (fromBodyAttr != null)
        {
            var body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync(cancellationToken);
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(body);
                    var propertyName = !string.IsNullOrEmpty(fromBodyAttr.Name) ? fromBodyAttr.Name : property.Name;
                    
                    if (jsonDoc.RootElement.TryGetProperty(propertyName, out var jsonElement))
                    {
                        return ConvertJsonElement(jsonElement, property.PropertyType);
                    }
                    
                    // If no specific property name found, try to deserialize the entire body to the property type
                    if (string.IsNullOrEmpty(fromBodyAttr.Name))
                    {
                        return JsonSerializer.Deserialize(body, property.PropertyType);
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, return null
                    return null;
                }
            }
        }

        // Check for FromServices attribute
        var fromServicesAttr = property.GetCustomAttribute<FromServicesAttribute>();
        if (fromServicesAttr != null)
        {
            return httpContext.RequestServices.GetService(property.PropertyType);
        }

        // Default binding logic (fallback)
        return await GetDefaultPropertyValueAsync(httpContext, property, cancellationToken);
    }

    private static async Task<object?> GetDefaultPropertyValueAsync(
        HttpContext httpContext,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
    {
        var propertyName = property.Name;

        // Try route values first
        if (httpContext.Request.RouteValues.TryGetValue(propertyName, out var routeValue))
        {
            return ConvertValue(routeValue?.ToString(), property.PropertyType);
        }

        // Try query parameters
        if (httpContext.Request.Query.TryGetValue(propertyName, out var queryValue))
        {
            return ConvertValue(queryValue.ToString(), property.PropertyType);
        }

        // Try headers
        if (httpContext.Request.Headers.TryGetValue(propertyName, out var headerValue))
        {
            return ConvertValue(headerValue.ToString(), property.PropertyType);
        }

        // Try form data if available
        if (httpContext.Request.HasFormContentType)
        {
            var form = await httpContext.Request.ReadFormAsync(cancellationToken);
            if (form.TryGetValue(propertyName, out var formValue))
            {
                return ConvertValue(formValue.ToString(), property.PropertyType);
            }
        }

        // Try JSON body for complex types
        if (httpContext.Request.ContentType?.Contains("application/json") == true)
        {
            httpContext.Request.Body.Position = 0; // Reset stream position
            var body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync(cancellationToken);
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(body);
                    
                    if (jsonDoc.RootElement.TryGetProperty(propertyName, out var jsonElement))
                    {
                        return ConvertJsonElement(jsonElement, property.PropertyType);
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, continue to other binding methods
                }
            }
        }

        return null;
    }

    private static object? ConvertValue(string? value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
            return value;

        if (underlyingType == typeof(int) && int.TryParse(value, out var intValue))
            return intValue;

        if (underlyingType == typeof(long) && long.TryParse(value, out var longValue))
            return longValue;

        if (underlyingType == typeof(double) && double.TryParse(value, out var doubleValue))
            return doubleValue;

        if (underlyingType == typeof(decimal) && decimal.TryParse(value, out var decimalValue))
            return decimalValue;

        if (underlyingType == typeof(bool) && bool.TryParse(value, out var boolValue))
            return boolValue;

        if (underlyingType == typeof(DateTime) && DateTime.TryParse(value, out var dateTimeValue))
            return dateTimeValue;

        if (underlyingType == typeof(DateTimeOffset) && DateTimeOffset.TryParse(value, out var dateTimeOffsetValue))
            return dateTimeOffsetValue;

        if (underlyingType == typeof(Guid) && Guid.TryParse(value, out var guidValue))
            return guidValue;

        if (underlyingType.IsEnum && Enum.TryParse(underlyingType, value, true, out var enumValue))
            return enumValue;

        // Try to use JsonSerializer for complex types
        try
        {
            return JsonSerializer.Deserialize($"\"{value}\"", targetType);
        }
        catch
        {
            return null;
        }
    }

    private static object? ConvertJsonElement(JsonElement jsonElement, Type targetType)
    {
        try
        {
            return JsonSerializer.Deserialize(jsonElement.GetRawText(), targetType);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
