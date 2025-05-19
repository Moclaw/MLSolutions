using System.Text.Json;
using MinimalAPI.Endpoints;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace MinimalAPI.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Binds request data from HTTP context to request object
    /// </summary>
    public static async Task<TRequest> BindAsync<TRequest>(this HttpContext httpContext, CancellationToken cancellationToken) 
        where TRequest : class, new()
    {
        var request = new TRequest();
        var properties = typeof(TRequest).GetProperties();

        // Bind route values
        foreach (var property in properties)
        {
            if (httpContext.Request.RouteValues.TryGetValue(property.Name.ToLower(), out var routeValue))
            {
                SetPropertyValue(property, request, routeValue);
            }
        }

        // Bind query string parameters
        foreach (var property in properties)
        {
            var value = httpContext.Request.Query[property.Name];
            if (!string.IsNullOrEmpty(value))
            {
                SetPropertyValue(property, request, value);
            }
        }

        // Bind body content for POST, PUT, PATCH requests
        if (httpContext.Request.ContentLength > 0 && 
            (httpContext.Request.Method == "POST" || 
             httpContext.Request.Method == "PUT" || 
             httpContext.Request.Method == "PATCH"))
        {
            if (httpContext.Request.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    var bodyDoc = await JsonDocument.ParseAsync(
                        httpContext.Request.Body, 
                        default, 
                        cancellationToken
                    );
                    
                    foreach (var property in properties)
                    {
                        if (bodyDoc.RootElement.TryGetProperty(property.Name, out var element) ||
                            bodyDoc.RootElement.TryGetProperty(property.Name.ToLower(), out element) ||
                            bodyDoc.RootElement.TryGetProperty(char.ToLower(property.Name[0]) + property.Name.Substring(1), out element))
                        {
                            SetPropertyFromJsonElement(property, request, element);
                        }
                    }
                }
                catch (JsonException) 
                {
                    // Log or handle JSON parsing errors
                }
            }
        }

        return request;
    }

    private static void SetPropertyValue(PropertyInfo property, object target, object? value)
    {
        if (value == null) return;

        try
        {
            if (property.PropertyType == typeof(int) && int.TryParse(value.ToString(), out var intValue))
            {
                property.SetValue(target, intValue);
            }
            else if (property.PropertyType == typeof(bool) && bool.TryParse(value.ToString(), out var boolValue))
            {
                property.SetValue(target, boolValue);
            }
            else if (property.PropertyType == typeof(DateTime) && DateTime.TryParse(value.ToString(), out var dateValue))
            {
                property.SetValue(target, dateValue);
            }
            else if (property.PropertyType == typeof(string))
            {
                property.SetValue(target, value.ToString());
            }
            // Add other type conversions as needed
        }
        catch (Exception)
        {
            // Log or handle conversion errors
        }
    }

    private static void SetPropertyFromJsonElement(PropertyInfo property, object target, JsonElement element)
    {
        try
        {
            if (property.PropertyType == typeof(int) && element.ValueKind == JsonValueKind.Number)
            {
                property.SetValue(target, element.GetInt32());
            }
            else if (property.PropertyType == typeof(bool) && element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
            {
                property.SetValue(target, element.GetBoolean());
            }
            else if (property.PropertyType == typeof(DateTime) && element.ValueKind == JsonValueKind.String)
            {
                if (element.TryGetDateTime(out var dateValue))
                {
                    property.SetValue(target, dateValue);
                }
            }
            else if (property.PropertyType == typeof(string) && 
                     (element.ValueKind == JsonValueKind.String || element.ValueKind == JsonValueKind.Number))
            {
                property.SetValue(target, element.GetString());
            }
            // Add other type conversions as needed
        }
        catch (Exception)
        {
            // Log or handle conversion errors
        }
    }
}
