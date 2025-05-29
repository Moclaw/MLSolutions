using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace MinimalAPI.SwaggerUI;

/// <summary>
/// Extension methods for configuring SwaggerUI with versioning support
/// </summary>
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
        params Assembly[] assemblies
    )
    {
        services.AddMinimalApi(assemblies);

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
            foreach (var assembly in assemblies)
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
            EndpointAssemblies = assemblies,
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
#pragma warning disable IDE0060 // Remove unused parameter
        bool enableTryItOut = true,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        bool enableDeepLinking = true,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        bool enableFilter = true,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        bool enableValidator = false,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        DocExpansion docExpansion = DocExpansion.List,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        ModelRendering defaultModelRendering = ModelRendering.Example,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        bool persistAuthorization = true
#pragma warning restore IDE0060 // Remove unused parameter
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

    /// <summary>
    /// Configure SwaggerUI with versioned endpoint support
    /// </summary>
    public static IApplicationBuilder UseMinimalApiSwaggerUI(
        this IApplicationBuilder app,
        string routePrefix = "swagger",
        bool enableTryItOut = true,
        bool enableDeepLinking = true,
        bool enableFilter = true,
        bool enableValidator = false,
        DocExpansion docExpansion = DocExpansion.List,
        ModelRendering defaultModelRendering = ModelRendering.Example,
        bool persistAuthorization = true,
        VersioningOptions? versioningOptions = null)
    {
        var options = app.ApplicationServices.GetService<SwaggerUIOptions>();
        var versioning = versioningOptions ?? new DefaultVersioningOptions();

        app.UseSwaggerUI(c =>
        {
            // Configure multiple swagger endpoints for each version
            foreach (var version in versioning.SupportedVersions)
            {
                var versionName = $"{versioning.Prefix}{version}";
                c.SwaggerEndpoint($"/swagger/{versionName}/swagger.json", $"{options?.Title ?? "API"} {versionName}");
            }

            c.RoutePrefix = routePrefix;
            //c.EnableTryItOutByDefault(enableTryItOut);
            //c.EnableDeepLinking(enableDeepLinking);
            //c.EnableFilter(enableFilter);
            //c.EnableValidator(enableValidator);
            c.DocExpansion(docExpansion);
            c.DefaultModelRendering(defaultModelRendering);
            c.DefaultModelExpandDepth(2);
            c.DefaultModelsExpandDepth(1);
            c.DisplayOperationId();
            c.DisplayRequestDuration();
            
            if (persistAuthorization)
            {
                c.EnablePersistAuthorization();
            }

            // Custom CSS and JavaScript for enhanced UI
            c.InjectStylesheet("/swagger-ui/custom.css");
            c.InjectJavascript("/swagger-ui/custom.js");
        });

        return app;
    }

    /// <summary>
    /// Add enhanced documentation with versioning
    /// </summary>
    public static IApplicationBuilder UseMinimalApiDocs(
        this IApplicationBuilder app,
        string swaggerRoutePrefix = "docs",
        bool enableTryItOut = true,
        bool enableDeepLinking = true,
        bool enableFilter = true,
        bool enableValidator = false,
        VersioningOptions? versioningOptions = null)
    {
        var versioning = versioningOptions ?? new DefaultVersioningOptions();

        // Enable Swagger JSON endpoints for each version
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
            c.PreSerializeFilters.Add((swagger, httpReq) =>
            {
                swagger.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
                {
                    new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
                };
            });
        });

        // Configure SwaggerUI with versioning
        app.UseMinimalApiSwaggerUI(
            routePrefix: swaggerRoutePrefix,
            enableTryItOut: enableTryItOut,
            enableDeepLinking: enableDeepLinking,
            enableFilter: enableFilter,
            enableValidator: enableValidator,
            versioningOptions: versioning
        );

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
