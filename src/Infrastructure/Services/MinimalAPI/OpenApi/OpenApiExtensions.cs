using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;

namespace MinimalAPI.OpenApi;

public static class OpenApiExtensions
{
    /// <summary>
    /// Adds enhanced OpenAPI documentation with custom parameter support
    /// </summary>
    public static IServiceCollection AddMinimalApiOpenApi(
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
}

public class OpenApiOptions
{
    public string Title { get; set; } = "API";
    public string Version { get; set; } = "v1";
    public string? Description { get; set; }
    public Assembly[] EndpointAssemblies { get; set; } = [];
}
