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
        var properties = typeof(TRequest).GetProperties(
            BindingFlags.Public | BindingFlags.Instance
        );

        foreach (var property in properties)
        {
            // Skip service injection properties
            if (property.GetCustomAttribute<FromServicesAttribute>() != null)
                continue;

            await BindPropertyAsync(httpContext, request, property, cancellationToken);
        }

        return request;
    }

    private static async Task BindPropertyAsync<T>(
        HttpContext httpContext,
        T request,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
        where T : class
    {
        // Check for specific From attributes
        var fromRouteAttr = property.GetCustomAttribute<FromRouteAttribute>();
        var fromQueryAttr = property.GetCustomAttribute<FromQueryAttribute>();
        var fromBodyAttr = property.GetCustomAttribute<FromBodyAttribute>();
        var fromHeaderAttr = property.GetCustomAttribute<FromHeaderAttribute>();
        var fromFormAttr = property.GetCustomAttribute<FromFormAttribute>();

        // If FromRoute attribute is present, bind from route values
        if (fromRouteAttr != null)
        {
            BindFromRoute(httpContext, request, property, fromRouteAttr.Name);
            return;
        }

        // If FromQuery attribute is present, bind from query string
        if (fromQueryAttr != null)
        {
            BindFromQuery(httpContext, request, property, fromQueryAttr.Name);
            return;
        }

        // If FromHeader attribute is present, bind from headers
        if (fromHeaderAttr != null)
        {
            BindFromHeader(httpContext, request, property, fromHeaderAttr.Name);
            return;
        }

        // If FromForm attribute is present or it's an IFormFile, bind from form data
        if (fromFormAttr != null || IsFormFileProperty(property))
        {
            await BindFromFormAsync(httpContext, request, property, fromFormAttr?.Name);
            return;
        }

        // If FromBody attribute is present, bind from request body
        if (fromBodyAttr != null)
        {
            await BindFromBodyAsync(httpContext, request, property, cancellationToken);
            return;
        }

        // Default binding logic
        await DefaultBindingAsync(httpContext, request, property, cancellationToken);
    }

    private static bool IsFormFileProperty(PropertyInfo property)
    {
        return property.PropertyType == typeof(IFormFile)
            || property.PropertyType == typeof(IFormFile[])
            || (
                property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                && property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile)
            );
    }

    private static void BindFromRoute<T>(
        HttpContext httpContext,
        T request,
        PropertyInfo property,
        string? name
    )
        where T : class
    {
        var routeName = name ?? property.Name.ToLower();
        if (httpContext.Request.RouteValues.TryGetValue(routeName, out var routeValue))
        {
            SetPropertyValue(property, request, routeValue);
        }
    }

    private static void BindFromQuery<T>(
        HttpContext httpContext,
        T request,
        PropertyInfo property,
        string? name
    )
        where T : class
    {
        var queryName = name ?? property.Name;
        var value = httpContext.Request.Query[queryName];
        if (!string.IsNullOrEmpty(value))
        {
            SetPropertyValue(property, request, value);
        }
    }

    private static void BindFromHeader<T>(
        HttpContext httpContext,
        T request,
        PropertyInfo property,
        string? name
    )
        where T : class
    {
        var headerName = name ?? property.Name;
        var value = httpContext.Request.Headers[headerName];
        if (!string.IsNullOrEmpty(value))
        {
            SetPropertyValue(property, request, value);
        }
    }

    private static async Task BindFromFormAsync<T>(
        HttpContext httpContext,
        T request,
        PropertyInfo property,
        string? name
    )
        where T : class
    {
        if (!httpContext.Request.HasFormContentType)
            return;

        var form = await httpContext.Request.ReadFormAsync();
        var formName = name ?? property.Name;

        // Handle IFormFile types
        if (property.PropertyType == typeof(IFormFile))
        {
            var file = form.Files.GetFile(formName);
            if (file != null)
            {
                property.SetValue(request, file);
            }
        }
        else if (property.PropertyType == typeof(IFormFile[]))
        {
            var files = form.Files.GetFiles(formName);
            if (files.Count > 0)
            {
                property.SetValue(request, files.ToArray());
            }
        }
        else if (
            property.PropertyType.IsGenericType
            && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
            && property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile)
        )
        {
            var files = form.Files.GetFiles(formName);
            if (files.Count > 0)
            {
                property.SetValue(request, files.ToList());
            }
        }
        else if (form.TryGetValue(formName, out var value))
        {
            SetPropertyValue(property, request, value);
        }
    }

    private static async Task BindFromBodyAsync<T>(
        HttpContext httpContext,
        T request,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
        where T : class
    {
        if (
            httpContext.Request.ContentLength > 0
            && httpContext.Request.ContentType?.Contains("application/json") == true
        )
        {
            try
            {
                httpContext.Request.Body.Position = 0;
                var bodyDoc = await JsonDocument.ParseAsync(
                    httpContext.Request.Body,
                    default,
                    cancellationToken
                );

                if (
                    bodyDoc.RootElement.TryGetProperty(property.Name, out var element)
                    || bodyDoc.RootElement.TryGetProperty(property.Name.ToLower(), out element)
                    || bodyDoc.RootElement.TryGetProperty(
                        char.ToLower(property.Name[0]) + property.Name.Substring(1),
                        out element
                    )
                )
                {
                    SetPropertyFromJsonElement(property, request, element);
                }
            }
            catch (JsonException)
            {
                throw new InvalidOperationException(
                    $"Failed to bind property '{property.Name}' of type '{property.PropertyType}' from JSON body"
                );
            }
        }
    }

    private static async Task DefaultBindingAsync<T>(
        HttpContext httpContext,
        T request,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
        where T : class
    {
        // Try route values first
        if (
            httpContext.Request.RouteValues.TryGetValue(property.Name.ToLower(), out var routeValue)
        )
        {
            SetPropertyValue(property, request, routeValue);
            return;
        }

        // Try query string
        var queryValue = httpContext.Request.Query[property.Name];
        if (!string.IsNullOrEmpty(queryValue))
        {
            SetPropertyValue(property, request, queryValue);
            return;
        }

        // Try form data for form content type
        if (httpContext.Request.HasFormContentType)
        {
            await BindFromFormAsync(httpContext, request, property, null);
            return;
        }

        // Try body for JSON content
        if (
            httpContext.Request.ContentLength > 0
            && httpContext.Request.ContentType?.Contains("application/json") == true
        )
        {
            await BindFromBodyAsync(httpContext, request, property, cancellationToken);
        }
    }

    private static void SetPropertyValue<T>(PropertyInfo property, T target, object? value)
        where T : class
    {
        if (value == null)
            return;

        try
        {
            var propertyType =
                Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var stringValue = value.ToString();

            if (string.IsNullOrEmpty(stringValue) && propertyType != typeof(string))
                return;

            object? convertedValue = propertyType switch
            {
                var t when t == typeof(int)
                    => int.TryParse(stringValue, out var intValue) ? intValue : null,
                var t when t == typeof(long)
                    => long.TryParse(stringValue, out var longValue) ? longValue : null,
                var t when t == typeof(double)
                    => double.TryParse(stringValue, out var doubleValue) ? doubleValue : null,
                var t when t == typeof(decimal)
                    => decimal.TryParse(stringValue, out var decimalValue) ? decimalValue : null,
                var t when t == typeof(bool)
                    => bool.TryParse(stringValue, out var boolValue)
                        ? boolValue
                        : stringValue == "1" || stringValue.ToLower() == "on"
                            ? true
                            : stringValue == "0" || stringValue.ToLower() == "off"
                                ? false
                                : null,
                var t when t == typeof(DateTime)
                    => DateTime.TryParse(stringValue, out var dateValue) ? dateValue : null,
                var t when t == typeof(DateTimeOffset)
                    => DateTimeOffset.TryParse(stringValue, out var dateOffsetValue)
                        ? dateOffsetValue
                        : null,
                var t when t == typeof(Guid)
                    => Guid.TryParse(stringValue, out var guidValue) ? guidValue : null,
                var t when t.IsEnum
                    => Enum.TryParse(propertyType, stringValue, true, out var enumValue)
                        ? enumValue
                        : null,
                var t when t == typeof(string) => stringValue,
                _ => null
            };

            if (convertedValue != null || Nullable.GetUnderlyingType(property.PropertyType) != null)
            {
                property.SetValue(target, convertedValue);
            }
        }
        catch (Exception)
        {
            throw new InvalidOperationException(
                $"Failed to bind property '{property.Name}' of type '{property.PropertyType}' with value '{value}'"
            );
        }
    }

    private static void SetPropertyFromJsonElement<T>(
        PropertyInfo property,
        T target,
        JsonElement element
    )
        where T : class
    {
        try
        {
            var propertyType =
                Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            object? value = propertyType switch
            {
                var t when t == typeof(int) && element.ValueKind == JsonValueKind.Number
                    => element.GetInt32(),
                var t when t == typeof(long) && element.ValueKind == JsonValueKind.Number
                    => element.GetInt64(),
                var t when t == typeof(double) && element.ValueKind == JsonValueKind.Number
                    => element.GetDouble(),
                var t when t == typeof(decimal) && element.ValueKind == JsonValueKind.Number
                    => element.GetDecimal(),
                var t
                    when t == typeof(bool)
                        && (
                            element.ValueKind == JsonValueKind.True
                            || element.ValueKind == JsonValueKind.False
                        )
                    => element.GetBoolean(),
                var t when t == typeof(DateTime) && element.ValueKind == JsonValueKind.String
                    => element.TryGetDateTime(out var dateValue) ? dateValue : null,
                var t when t == typeof(DateTimeOffset) && element.ValueKind == JsonValueKind.String
                    => element.TryGetDateTimeOffset(out var dateOffsetValue)
                        ? dateOffsetValue
                        : null,
                var t when t == typeof(Guid) && element.ValueKind == JsonValueKind.String
                    => element.TryGetGuid(out var guidValue) ? guidValue : null,
                var t when t.IsEnum && element.ValueKind == JsonValueKind.String
                    => Enum.TryParse(propertyType, element.GetString(), true, out var enumValue)
                        ? enumValue
                        : null,
                var t when t == typeof(string) => element.GetString(),
                _ => null
            };

            if (value != null || Nullable.GetUnderlyingType(property.PropertyType) != null)
            {
                property.SetValue(target, value);
            }
        }
        catch (Exception)
        {
            throw new InvalidOperationException(
                $"Failed to bind property '{property.Name}' of type '{property.PropertyType}'"
            );
        }
    }
}
