using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using MinimalAPI.Attributes;

namespace MinimalAPI.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Binds request data from HTTP context to request object
    /// </summary>
    public static async Task<TRequest> BindAsync<TRequest>(
        this HttpContext httpContext,
        CancellationToken cancellationToken
    )
        where TRequest : class, new()
    {
        var request = new TRequest();
        var properties = typeof(TRequest).GetProperties();

        foreach (var property in properties)
        {
            await BindPropertyAsync(httpContext, request, property, cancellationToken);
        }

        return request;
    }

    private static async Task BindPropertyAsync(
        HttpContext httpContext,
        object request,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
    {
        // Check for specific From attributes
        var fromRouteAttr = property.GetCustomAttribute<FromRouteAttribute>();
        var fromQueryAttr = property.GetCustomAttribute<FromQueryAttribute>();
        var fromBodyAttr = property.GetCustomAttribute<FromBodyAttribute>();
        var fromHeaderAttr = property.GetCustomAttribute<FromHeaderAttribute>();
        var fromFormAttr = property.GetCustomAttribute<FromFormAttribute>();
        var fromServicesAttr = property.GetCustomAttribute<FromServicesAttribute>();

        // If FromServices attribute is present, get from DI container
        if (fromServicesAttr != null)
        {
            var service = httpContext.RequestServices.GetService(property.PropertyType);
            if (service != null)
            {
                property.SetValue(request, service);
            }
            return;
        }

        // If FromRoute attribute is present, bind from route values
        if (fromRouteAttr != null)
        {
            BindFromRoute(httpContext, request, property);
            return;
        }

        // If FromQuery attribute is present, bind from query string
        if (fromQueryAttr != null)
        {
            BindFromQuery(httpContext, request, property);
            return;
        }

        // If FromHeader attribute is present, bind from headers
        if (fromHeaderAttr != null)
        {
            BindFromHeader(httpContext, request, property);
            return;
        }

        // If FromForm attribute is present, bind from form data
        if (fromFormAttr != null)
        {
            await BindFromFormAsync(httpContext, request, property);
            return;
        }

        // If FromBody attribute is present, bind from request body
        if (fromBodyAttr != null)
        {
            await BindFromBodyAsync(httpContext, request, property, cancellationToken);
            return;
        }

        // Default binding logic (try route, then query, then body for applicable methods)
        await DefaultBindingAsync(httpContext, request, property, cancellationToken);
    }

    private static void BindFromRoute(
        HttpContext httpContext,
        object request,
        PropertyInfo property
    )
    {
        if (
            httpContext.Request.RouteValues.TryGetValue(property.Name.ToLower(), out var routeValue)
        )
        {
            SetPropertyValue(property, request, routeValue);
        }
    }

    private static void BindFromQuery(
        HttpContext httpContext,
        object request,
        PropertyInfo property
    )
    {
        var value = httpContext.Request.Query[property.Name];
        if (!string.IsNullOrEmpty(value))
        {
            SetPropertyValue(property, request, value);
        }
    }

    private static void BindFromHeader(
        HttpContext httpContext,
        object request,
        PropertyInfo property
    )
    {
        var value = httpContext.Request.Headers[property.Name];
        if (!string.IsNullOrEmpty(value))
        {
            SetPropertyValue(property, request, value);
        }
    }

    private static async Task BindFromFormAsync(
        HttpContext httpContext,
        object request,
        PropertyInfo property
    )
    {
        if (httpContext.Request.HasFormContentType)
        {
            var form = await httpContext.Request.ReadFormAsync();

            // Handle IFormFile types
            if (property.PropertyType == typeof(IFormFile))
            {
                var file = form.Files.GetFile(property.Name);
                if (file != null)
                {
                    property.SetValue(request, file);
                }
            }
            else if (property.PropertyType == typeof(IFormFile[]))
            {
                var files = form.Files.GetFiles(property.Name);
                if (files != null && files.Count > 0)
                {
                    property.SetValue(request, files.ToArray());
                }
            }
            else if (property.PropertyType == typeof(List<IFormFile>))
            {
                var files = form.Files.GetFiles(property.Name);
                if (files != null && files.Count > 0)
                {
                    property.SetValue(request, files.ToList());
                }
            }
            else if (form.TryGetValue(property.Name, out var value))
            {
                SetPropertyValue(property, request, value);
            }
        }
    }

    private static async Task BindFromBodyAsync(
        HttpContext httpContext,
        object request,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
    {
        if (
            httpContext.Request.ContentLength > 0
            && httpContext.Request.ContentType?.Contains("application/json") == true
        )
        {
            try
            {
                httpContext.Request.Body.Position = 0; // Reset position if possible
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
                // Log or handle JSON parsing errors
            }
        }
    }

    private static async Task DefaultBindingAsync(
        HttpContext httpContext,
        object request,
        PropertyInfo property,
        CancellationToken cancellationToken
    )
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

        // Try form data for POST, PUT, PATCH requests with form content type
        if (
            httpContext.Request.HasFormContentType
            && (
                httpContext.Request.Method == "POST"
                || httpContext.Request.Method == "PUT"
                || httpContext.Request.Method == "PATCH"
            )
        )
        {
            await BindFromFormAsync(httpContext, request, property);
            return;
        }

        // Try body for POST, PUT, PATCH requests with JSON content
        if (
            httpContext.Request.ContentLength > 0
            && (
                httpContext.Request.Method == "POST"
                || httpContext.Request.Method == "PUT"
                || httpContext.Request.Method == "PATCH"
            )
        )
        {
            if (httpContext.Request.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    httpContext.Request.Body.Position = 0; // Reset position if possible
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
                    // Log or handle JSON parsing errors
                }
            }
        }
    }

    private static void SetPropertyValue(PropertyInfo property, object target, object? value)
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

            object? convertedValue = null;

            if (propertyType == typeof(int))
            {
                if (int.TryParse(stringValue, out var intValue))
                    convertedValue = intValue;
            }
            else if (propertyType == typeof(long))
            {
                if (long.TryParse(stringValue, out var longValue))
                    convertedValue = longValue;
            }
            else if (propertyType == typeof(double))
            {
                if (double.TryParse(stringValue, out var doubleValue))
                    convertedValue = doubleValue;
            }
            else if (propertyType == typeof(decimal))
            {
                if (decimal.TryParse(stringValue, out var decimalValue))
                    convertedValue = decimalValue;
            }
            else if (propertyType == typeof(bool))
            {
                if (bool.TryParse(stringValue, out var boolValue))
                    convertedValue = boolValue;
                else if (stringValue == "1" || stringValue?.ToLower() == "on")
                    convertedValue = true;
                else if (stringValue == "0" || stringValue?.ToLower() == "off")
                    convertedValue = false;
            }
            else if (propertyType == typeof(DateTime))
            {
                if (DateTime.TryParse(stringValue, out var dateValue))
                    convertedValue = dateValue;
            }
            else if (propertyType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(stringValue, out var dateOffsetValue))
                    convertedValue = dateOffsetValue;
            }
            else if (propertyType == typeof(Guid))
            {
                if (Guid.TryParse(stringValue, out var guidValue))
                    convertedValue = guidValue;
            }
            else if (propertyType.IsEnum)
            {
                if (Enum.TryParse(propertyType, stringValue, true, out var enumValue))
                    convertedValue = enumValue;
            }
            else if (propertyType == typeof(string))
            {
                convertedValue = stringValue;
            }

            if (convertedValue != null || Nullable.GetUnderlyingType(property.PropertyType) != null)
            {
                property.SetValue(target, convertedValue);
            }
        }
        catch (Exception)
        {
            // Log or handle conversion errors
        }
    }

    private static void SetPropertyFromJsonElement(
        PropertyInfo property,
        object target,
        JsonElement element
    )
    {
        try
        {
            var propertyType =
                Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (propertyType == typeof(int) && element.ValueKind == JsonValueKind.Number)
            {
                property.SetValue(target, element.GetInt32());
            }
            else if (propertyType == typeof(long) && element.ValueKind == JsonValueKind.Number)
            {
                property.SetValue(target, element.GetInt64());
            }
            else if (propertyType == typeof(double) && element.ValueKind == JsonValueKind.Number)
            {
                property.SetValue(target, element.GetDouble());
            }
            else if (propertyType == typeof(decimal) && element.ValueKind == JsonValueKind.Number)
            {
                property.SetValue(target, element.GetDecimal());
            }
            else if (
                propertyType == typeof(bool)
                && (
                    element.ValueKind == JsonValueKind.True
                    || element.ValueKind == JsonValueKind.False
                )
            )
            {
                property.SetValue(target, element.GetBoolean());
            }
            else if (propertyType == typeof(DateTime) && element.ValueKind == JsonValueKind.String)
            {
                if (element.TryGetDateTime(out var dateValue))
                {
                    property.SetValue(target, dateValue);
                }
            }
            else if (
                propertyType == typeof(DateTimeOffset) && element.ValueKind == JsonValueKind.String
            )
            {
                if (element.TryGetDateTimeOffset(out var dateOffsetValue))
                {
                    property.SetValue(target, dateOffsetValue);
                }
            }
            else if (propertyType == typeof(Guid) && element.ValueKind == JsonValueKind.String)
            {
                if (element.TryGetGuid(out var guidValue))
                {
                    property.SetValue(target, guidValue);
                }
            }
            else if (propertyType.IsEnum && element.ValueKind == JsonValueKind.String)
            {
                var stringValue = element.GetString();
                if (
                    !string.IsNullOrEmpty(stringValue)
                    && Enum.TryParse(propertyType, stringValue, true, out var enumValue)
                )
                {
                    property.SetValue(target, enumValue);
                }
            }
            else if (
                propertyType == typeof(string)
                && (
                    element.ValueKind == JsonValueKind.String
                    || element.ValueKind == JsonValueKind.Number
                )
            )
            {
                property.SetValue(target, element.GetString());
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
