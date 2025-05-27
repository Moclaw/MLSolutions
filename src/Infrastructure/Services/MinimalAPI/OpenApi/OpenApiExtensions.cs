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

public static class OpenApiExtensions
{
    /// <summary>
    /// Adds enhanced OpenAPI documentation with custom parameter support
    /// </summary>
    public static IServiceCollection AddMinimalApiWithOpenApi(
        this IServiceCollection services,
        string title = "API",
        string version = "v1",
        string? description = null,
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
        params Assembly[] endpointAssemblies
    )
    {
        // Add SwaggerUI (which includes Swagger generation)
        services.AddMinimalApiSwaggerUI(
            title, version, description, 
            contactName, contactEmail, contactUrl,
            licenseName, licenseUrl,
            endpointAssemblies);

        // Also add OpenAPI options for compatibility
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
    /// Uses enhanced OpenAPI with custom documentation
    /// </summary>
    public static WebApplication UseMinimalApiOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

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
        bool enableFilter = true
    )
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            // Enable SwaggerUI with custom assets
            app.UseMinimalApiSwaggerUI(
                routePrefix: swaggerRoutePrefix,
                enableTryItOut: enableTryItOut,
                enableDeepLinking: enableDeepLinking,
                enableFilter: enableFilter
            );
        }

        return app;
    }
}

public class OpenApiOptions
{
    public string Title { get; set; } = "API";
    public string Version { get; set; } = "v1";
    public string? Description { get; set; }
    public Assembly[] EndpointAssemblies { get; set; } = [];
}
