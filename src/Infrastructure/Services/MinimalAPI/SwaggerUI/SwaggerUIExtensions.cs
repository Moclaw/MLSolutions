using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace MinimalAPI.SwaggerUI;

public static class SwaggerUIExtensions
{
    /// <summary>
    /// Adds SwaggerUI with enhanced configuration for MinimalAPI
    /// </summary>
    public static IServiceCollection AddMinimalApiSwaggerUI(
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
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = description,
                Contact = !string.IsNullOrEmpty(contactName) ? new OpenApiContact
                {
                    Name = contactName,
                    Email = contactEmail,
                    Url = !string.IsNullOrEmpty(contactUrl) ? new Uri(contactUrl) : null
                } : null,
                License = !string.IsNullOrEmpty(licenseName) ? new OpenApiLicense
                {
                    Name = licenseName,
                    Url = !string.IsNullOrEmpty(licenseUrl) ? new Uri(licenseUrl) : null
                } : null
            });

            // Add XML comments if available
            foreach (var assembly in endpointAssemblies)
            {
                var xmlFile = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }

            // Add security definition for Bearer token
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Add custom document filter for MinimalAPI
            options.DocumentFilter<MinimalApiDocumentFilter>();
            
            // Add operation filter for better parameter handling
            options.OperationFilter<MinimalApiOperationFilter>();
        });

        services.AddSingleton(new SwaggerUIOptions
        {
            Title = title,
            Version = version,
            Description = description,
            EndpointAssemblies = endpointAssemblies,
            ContactName = contactName,
            ContactEmail = contactEmail,
            ContactUrl = contactUrl,
            LicenseName = licenseName,
            LicenseUrl = licenseUrl
        });

        return services;
    }

    /// <summary>
    /// Uses SwaggerUI with enhanced configuration
    /// </summary>
    public static WebApplication UseMinimalApiSwaggerUI(
        this WebApplication app,
        string? routePrefix = null,
        bool enableTryItOut = true,
        bool enableDeepLinking = true,
        bool enableFilter = true,
        bool enableValidator = false,
        DocExpansion docExpansion = DocExpansion.List,
        ModelRendering defaultModelRendering = ModelRendering.Example,
        bool persistAuthorization = true
    )
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            var swaggerOptions = app.Services.GetService<SwaggerUIOptions>();
            var version = swaggerOptions?.Version ?? "v1";

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{version}/swagger.json", 
                    $"{swaggerOptions?.Title ?? "API"} {version}");
                
                if (!string.IsNullOrEmpty(routePrefix))
                {
                    options.RoutePrefix = routePrefix;
                }
            });
        }

        return app;
    }

}

public class SwaggerUIOptions
{
    public string Title { get; set; } = "API";
    public string Version { get; set; } = "v1";
    public string? Description { get; set; }
    public Assembly[] EndpointAssemblies { get; set; } = [];
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactUrl { get; set; }
    public string? LicenseName { get; set; }
    public string? LicenseUrl { get; set; }
}
