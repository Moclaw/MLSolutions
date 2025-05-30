using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.Attributes;
using MinimalAPI.OpenApi;
using System.Reflection;

namespace MinimalAPI;

public static partial class Register
{
    /// <summary>
    /// Adds MinimalAPI services and MediatR to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for endpoint handlers</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMinimalApi(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        // Register MediatR handlers from all assemblies
        // Find all potential application assemblies that might contain handlers
        var allAssemblies = new List<Assembly>(assemblies);

        // Add references to application assemblies that contain handlers
        foreach (var assembly in assemblies)
        {
            var referencedAssemblies = assembly
                .GetReferencedAssemblies()
                .Where(a =>
                    a.Name?.Contains("Application") == true
                    || a.Name?.Contains("Features") == true
                    || a.Name?.Contains("Handlers") == true
                )
                .Select(Assembly.Load);

            allAssemblies.AddRange(referencedAssemblies);
        }

        // Register MediatR services
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies([.. allAssemblies.Distinct()])
        );

        return services;
    }

    /// <summary>
    /// Adds MinimalAPI services with enhanced OpenAPI documentation
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="title">API title</param>
    /// <param name="version">API version</param>
    /// <param name="description">API description</param>
    /// <param name="assemblies">Assemblies to scan for endpoint handlers</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMinimalApiWithOpenApi(
        this IServiceCollection services,
        string title = "API",
        string version = "v1",
        string? description = null,
        DefaultVersioningOptions? versioningOptions = null,
        params Assembly[] assemblies
    )
    {
        // Add base MinimalAPI services
        services.AddMinimalApi(assemblies);

        // Register versioning options
        var versioning = versioningOptions ?? new DefaultVersioningOptions();
        services.AddSingleton(versioning);

        // Add enhanced OpenAPI documentation with versioning
        services.AddOpenAPIDocument(title, version, description, versioningOptions: versioning, assemblies);

        return services;
    }

    /// <summary>
    /// Maps all endpoints from classes implementing IEndpointHandler found in the specified assemblies
    /// </summary>
    /// <param name="app">The WebApplication</param>
    /// <param name="assemblies">Assemblies to scan for endpoint handlers</param>
    /// <returns>The WebApplication</returns>
    public static WebApplication MapMinimalEndpoints(
        this WebApplication app,
        VersioningOptions? versioningOptions = null,
        params Assembly[] assemblies
    )
    {
        var versioning = versioningOptions ?? app.Services.GetService<VersioningOptions>() ?? new DefaultVersioningOptions();

        // Find all endpoint classes (classes deriving from EndpointBase)
        var endpointTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                !t.IsAbstract
                && !t.IsInterface
                && t.IsAssignableTo(typeof(MinimalAPI.Endpoints.EndpointAbstractBase))
            )
            .ToList();

        foreach (var endpointType in endpointTypes)
        {
            // Find all handler methods with HTTP method attributes
            var methods = endpointType
                .GetMethods()
                .Where(m =>
                    m.GetCustomAttributes().Any(a => a is MinimalAPI.Attributes.HttpMethodAttribute)
                )
                .ToList();

            foreach (var method in methods)
            {
                // Get HTTP method attribute
                if (method
                        .GetCustomAttributes()
                        .FirstOrDefault(a => a is MinimalAPI.Attributes.HttpMethodAttribute) is not MinimalAPI.Attributes.HttpMethodAttribute httpMethodAttr || string.IsNullOrEmpty(httpMethodAttr.Route))
                    continue;

                // Get version from ApiVersion attribute or use default
                var apiVersionAttr = method.GetCustomAttribute<ApiVersionAttribute>() ??
                                   endpointType.GetCustomAttribute<ApiVersionAttribute>();
                var endpointVersion = apiVersionAttr?.Version ?? versioning.DefaultVersion;

                // Generate versioned route template
                var routeTemplate = versioning.GetVersionedRoute(httpMethodAttr.Route, endpointVersion);

                // Create the endpoint handler with versioning support
                var handler = app.MapMethods(
                    routeTemplate,
                    [httpMethodAttr.Method],
                    async (HttpContext context, IServiceProvider serviceProvider) =>
                    {
                        // Validate version if required
                        if (!ValidateVersion(context, versioning, endpointVersion))
                        {
                            return Results.BadRequest($"API version {endpointVersion} is not supported");
                        }

                        // Create endpoint instance from service provider
                        if (ActivatorUtilities.CreateInstance(serviceProvider, endpointType) is not MinimalAPI.Endpoints.EndpointAbstractBase endpoint)
                            return Results.StatusCode(500);

                        // Set HttpContext and configure endpoint definition
                        endpoint.HttpContext = context;

                        // Configure endpoint definition with route and version info
                        var requestType = GetRequestType(endpointType) ?? typeof(object);
                        var responseType = GetResponseType(endpointType) ?? typeof(object);

                        endpoint.Definition = new EndpointDefinition(endpointType, requestType, responseType)
                        {
                            RouteTemplate = routeTemplate,
                            Version = versioning,
                            Verbs = [httpMethodAttr.Method]
                        };

                        // Execute the endpoint
                        await endpoint.ExecuteAsync(context.RequestAborted);

                        // Return an empty result as the endpoint will have written to the response
                        return Results.Empty;
                    }
                );

                // Add endpoint metadata for OpenAPI documentation
                handler.WithName($"{endpointType.Name}_{httpMethodAttr.Method}_{endpointVersion}_{Guid.NewGuid()} ")
                       .WithTags(GenerateEndpointTags(endpointType, endpointVersion))
                       .WithOpenApi();
            }
        }

        return app;
    }

    private static bool ValidateVersion(HttpContext context, VersioningOptions versioning, int endpointVersion)
    {
        // Extract version from request based on strategy
        var requestVersion = ExtractVersionFromRequest(context, versioning);

        // If no version specified and we assume default, use endpoint version
        if (!requestVersion.HasValue && versioning.AssumeDefaultVersionWhenUnspecified)
        {
            return versioning.SupportedVersions.Contains(endpointVersion);
        }

        // Validate that requested version matches endpoint version
        return requestVersion == endpointVersion && versioning.SupportedVersions.Contains(endpointVersion);
    }

    private static int? ExtractVersionFromRequest(HttpContext context, VersioningOptions versioning)
    {
        // URL Segment version
        if (versioning.ReadingStrategy.HasFlag(VersionReadingStrategy.UrlSegment))
        {
            var version = versioning.ExtractVersionFromRoute(context.Request.Path);
            if (version.HasValue) return version;
        }

        // Query parameter version
        if (versioning.ReadingStrategy.HasFlag(VersionReadingStrategy.QueryString))
        {
            var queryVersion = context.Request.Query[versioning.QueryParameterName].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryVersion) &&
                decimal.TryParse(queryVersion, out var qVersion))
            {
                return (int)qVersion;
            }
        }

        // Header version
        if (versioning.ReadingStrategy.HasFlag(VersionReadingStrategy.Header))
        {
            var headerVersion = context.Request.Headers[versioning.VersionHeaderName].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerVersion) &&
                decimal.TryParse(headerVersion, out var hVersion))
            {
                return (int)hVersion;
            }
        }

        return null;
    }

    private static Type? GetRequestType(Type endpointType)
    {
        var baseType = endpointType.BaseType;
        while (baseType != null && !baseType.IsGenericType)
        {
            baseType = baseType.BaseType;
        }

        return baseType?.IsGenericType == true ? baseType.GetGenericArguments().FirstOrDefault() : null;
    }

    private static Type? GetResponseType(Type endpointType)
    {
        var baseType = endpointType.BaseType;
        while (baseType != null && !baseType.IsGenericType)
        {
            baseType = baseType.BaseType;
        }

        if (baseType?.IsGenericType == true)
        {
            var args = baseType.GetGenericArguments();
            return args.Length > 1 ? args[1] : null;
        }

        return null;
    }

    private static string[] GenerateEndpointTags(Type endpointType, int version)
    {
        var tags = new HashSet<string>();

        var namespaceParts = endpointType.Namespace?.Split('.') ?? [];
        var featureIndex = Array.FindIndex(namespaceParts, part =>
            part.Equals("Endpoints", StringComparison.OrdinalIgnoreCase));

        if (featureIndex >= 0 && featureIndex + 1 < namespaceParts.Length)
        {
            var featureName = namespaceParts[featureIndex + 1];

            if (featureIndex + 2 < namespaceParts.Length)
            {
                var operationType = namespaceParts[featureIndex + 2];
                tags.Add($"{featureName} {operationType}");
            }
        }

        return [.. tags];
    }
}
