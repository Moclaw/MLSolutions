using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using MinimalAPI.SwaggerUI;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MinimalAPI.OpenApi;

/// <summary>
/// OpenAPI configuration options
/// </summary>
public class OpenApiOptions
{
    public string Title { get; set; } = "API";
    public string Version { get; set; } = "v1";
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactUrl { get; set; }
    public string? LicenseName { get; set; }
    public string? LicenseUrl { get; set; }
    public Assembly[]? EndpointAssemblies { get; set; }
    public VersioningOptions? VersioningOptions { get; set; }
}

/// <summary>
/// Extensions for OpenAPI configuration with versioning
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds enhanced OpenAPI documentation with custom parameter support
    /// </summary>
    public static IServiceCollection AddOpenAPIDocument(
        this IServiceCollection services,
        string title = "API",
        string version = "v1",
        string? description = null,
        DefaultVersioningOptions versioningOptions = null,
        params Assembly[] endpointAssemblies
    )
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<MinimalApiDocumentTransformer>();
        });

        services.AddSingleton(new OpenApiOptions
        {
            Title = title,
            Version = version,
            Description = description,
            EndpointAssemblies = endpointAssemblies
        });

        return services;
    }

    /// <summary>
    /// Adds enhanced OpenAPI documentation with SwaggerUI integration
    /// </summary>
    public static IServiceCollection AddMinimalApiWithSwaggerUI(
        this IServiceCollection services,
        string title = "API",
        string version = "v1",
        string? description = null,
        string? contactName = null,
        string? contactEmail = null,
        string? contactUrl = null,
        string? licenseName = null,
        string? licenseUrl = null,
        DefaultVersioningOptions? versioningOptions = null,
        params Assembly[] assemblies
    )
    {
        // Add SwaggerUI (which includes Swagger generation)
        services.AddMinimalApiSwaggerUI(
            title, version, description,
            contactName, contactEmail, contactUrl,
            licenseName, licenseUrl,
            assemblies);

        // Also add OpenAPI options for compatibility
        services.AddSingleton(new OpenApiOptions
        {
            Title = title,
            Version = version,
            Description = description,
            EndpointAssemblies = assemblies
        });

        return services;
    }

    /// <summary>
    /// Uses enhanced OpenAPI with custom documentation
    /// <paramref name="prefix"/>   is the route prefix for the OpenAPI document
    /// </summary>
    public static WebApplication UseMinimalApiOpenApi(this WebApplication app,
        string prefix = "openapi/v1.json")
    {
        app.MapOpenApi(prefix);

        return app;
    }

    /// <summary>
    /// Uses complete MinimalAPI documentation with both OpenAPI and SwaggerUI
    /// </summary>
    public static WebApplication UseMinimalApiDocs(
        this WebApplication app,
        string? swaggerRoutePrefix = null,
        bool enableTryItOut = true,
        bool enableDeepLinking = true,
        bool enableFilter = true,
        bool enableValidator = false
    )
    {
        app.UseMinimalApiSwaggerUI(
               routePrefix: swaggerRoutePrefix,
               enableTryItOut: enableTryItOut,
               enableDeepLinking: enableDeepLinking,
               enableFilter: enableFilter,
               enableValidator: enableValidator
           );

        return app;
    }
}
