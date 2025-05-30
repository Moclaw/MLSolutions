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
        VersioningOptions? versioningOptions = null,
        params Assembly[] assemblies
    )
    {
        services.AddMinimalApi(assemblies);

        var versioning = versioningOptions ?? new DefaultVersioningOptions();

        services.AddSwaggerGen(options =>
        {
            // Generate separate documents for each supported version
            foreach (var supportedVersion in versioning.SupportedVersions)
            {
                var (docName, docTitle, docDescription) = versioning.GetSwaggerDocInfo(supportedVersion);
                
                options.SwaggerDoc(docName, new OpenApiInfo
                {
                    Title = $"{title}",
                    Version = docName,
                    Description = $"{description ?? docDescription}",
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
            }

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

            // Add document filter for version-specific filtering
            options.DocumentFilter<VersionedMinimalApiDocumentFilter>();
            
            // Add operation filter for better parameter handling and unique operation IDs
            options.OperationFilter<MinimalApiOperationFilter>();

            // Configure API explorer to include version in group names
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                // Extract version from route template first
                var route = apiDesc.RelativePath ?? "";
                var extractedVersion = versioning.ExtractVersionFromRoute(route);
                
                if (extractedVersion.HasValue)
                {
                    var expectedDocName = versioning.GetSwaggerDocName(extractedVersion.Value);
                    return docName.Equals(expectedDocName, StringComparison.OrdinalIgnoreCase);
                }

                // Check action descriptor route values as fallback
                if (apiDesc.ActionDescriptor.RouteValues.TryGetValue("version", out var routeVersion))
                {
                    return docName.Equals($"v{routeVersion}", StringComparison.OrdinalIgnoreCase);
                }

                // For routes without explicit version, include in all version documents
                // This ensures endpoints are available in all tabs if version detection fails
                return true;
            });

            // Ensure unique operation IDs across all endpoints
            options.CustomOperationIds(apiDesc =>
            {
                // Let the operation filter handle operation ID generation
                return null;
            });
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
            LicenseUrl = licenseUrl,
            VersioningOptions = versioning
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
            var versioning = swaggerOptions?.VersioningOptions ?? new DefaultVersioningOptions();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // Configure multiple swagger endpoints for each version with proper tab names
                foreach (var version in versioning.SupportedVersions)
                {
                    var versionName = versioning.GetSwaggerDocName(version);
                    var displayName = $"{swaggerOptions?.Title ?? "API"} {versionName.ToUpperInvariant()}";
                    
                    options.SwaggerEndpoint($"/swagger/{versionName}/swagger.json", displayName);
                }
                
                if (!string.IsNullOrEmpty(routePrefix))
                {
                    options.RoutePrefix = routePrefix;
                }

                // Enhanced UI configuration
                options.DocExpansion(docExpansion);
                options.DefaultModelRendering(defaultModelRendering);
                options.DefaultModelExpandDepth(2);
                options.DefaultModelsExpandDepth(1);
                options.DisplayOperationId();
                options.DisplayRequestDuration();
                
                if (persistAuthorization)
                {
                    options.EnablePersistAuthorization();
                }

                // Custom CSS and JavaScript for enhanced UI
                options.InjectStylesheet("/swagger-ui/custom.css");
                options.InjectJavascript("/swagger-ui/custom.js");
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
        var versioning = versioningOptions ?? options?.VersioningOptions ?? new DefaultVersioningOptions();

        app.UseSwaggerUI(c =>
        {
            // Configure multiple swagger endpoints for each version with enhanced tab names
            foreach (var version in versioning.SupportedVersions)
            {
                var versionName = versioning.GetSwaggerDocName(version);
                var displayName = $"{options?.Title ?? "API"} {versionName.ToUpperInvariant()}";
                
                c.SwaggerEndpoint($"/swagger/{versionName}/swagger.json", displayName);
            }

            c.RoutePrefix = routePrefix;
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
    public VersioningOptions? VersioningOptions { get; set; }
}
