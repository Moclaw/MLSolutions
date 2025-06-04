using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MinimalAPI.SwaggerUI;

public class MinimalApiDocumentFilter(SwaggerUIOptions options, IWebHostEnvironment environment, IConfiguration configuration) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Enhance document info
        swaggerDoc.Info.Title = options.Title;
        swaggerDoc.Info.Version = options.Version;
        swaggerDoc.Info.Description = options.Description;

        if (!string.IsNullOrEmpty(options.ContactName))
        {
            swaggerDoc.Info.Contact = new OpenApiContact
            {
                Name = options.ContactName,
                Email = options.ContactEmail,
                Url = !string.IsNullOrEmpty(options.ContactUrl)
                    ? new Uri(options.ContactUrl)
                    : null
            };
        }

        if (!string.IsNullOrEmpty(options.LicenseName))
        {
            swaggerDoc.Info.License = new OpenApiLicense
            {
                Name = options.LicenseName,
                Url = !string.IsNullOrEmpty(options.LicenseUrl)
                    ? new Uri(options.LicenseUrl)
                    : null
            };
        }

        // Add servers information from launch settings
        swaggerDoc.Servers = GetServersFromLaunchSettings();

        // Add dynamic tags based on discovered features
        AddDynamicTags(swaggerDoc);

        // Sort paths alphabetically
        var sortedPaths = swaggerDoc.Paths
            .OrderBy(p => p.Key)
            .ToDictionary(p => p.Key, p => p.Value);

        swaggerDoc.Paths.Clear();
        foreach (var path in sortedPaths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }

    private List<OpenApiServer> GetServersFromLaunchSettings()
    {
        var servers = new List<OpenApiServer>();

        try
        {
            // Get URLs from configuration (appsettings or launch settings)
            var urls = configuration.GetValue<string>("urls");
            if (!string.IsNullOrEmpty(urls))
            {
                var urlList = urls.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var url in urlList)
                {
                    servers.Add(new OpenApiServer 
                    { 
                        Url = url.Trim(), 
                        Description = $"{environment.EnvironmentName} server"
                    });
                }
            }

            // Try to read from launchSettings.json if no URLs in configuration
            if (servers.Count == 0)
            {
                var launchSettingsPath = Path.Combine(environment.ContentRootPath, "Properties", "launchSettings.json");
                if (File.Exists(launchSettingsPath))
                {
                    var launchSettingsJson = File.ReadAllText(launchSettingsPath);
                    using var document = JsonDocument.Parse(launchSettingsJson);
                    
                    if (document.RootElement.TryGetProperty("profiles", out var profiles))
                    {
                        foreach (var profile in profiles.EnumerateObject())
                        {
                            if (profile.Value.TryGetProperty("applicationUrl", out var applicationUrl))
                            {
                                var urlValue = applicationUrl.GetString();
                                if (!string.IsNullOrEmpty(urlValue))
                                {
                                    var urlList = urlValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var url in urlList)
                                    {
                                        servers.Add(new OpenApiServer 
                                        { 
                                            Url = url.Trim(), 
                                            Description = $"{profile.Name} - {environment.EnvironmentName}"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Fallback to default if no servers found
            if (servers.Count == 0)
            {
                servers.Add(new OpenApiServer 
                { 
                    Url = "/", 
                    Description = $"Current server - {environment.EnvironmentName}"
                });
            }
        }
        catch
        {
            // Fallback to default server on any error
            servers.Add(new OpenApiServer 
            { 
                Url = "/", 
                Description = $"Current server - {environment.EnvironmentName}"
            });
        }

        return servers;
    }

    private void AddDynamicTags(OpenApiDocument swaggerDoc)
    {
        // Collect all unique tag names from operations
        var operationTags = new HashSet<string>();
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                foreach (var tag in operation.Value.Tags)
                {
                    operationTags.Add(tag.Name);
                }
            }
        }

        // Add tag definitions
        if (swaggerDoc.Tags == null) swaggerDoc.Tags = new List<OpenApiTag>();
        
        foreach (var tagName in operationTags)
        {
            // Check if the tag already exists in document
            if (!swaggerDoc.Tags.Any(t => t.Name == tagName))
            {
                swaggerDoc.Tags.Add(new OpenApiTag 
                {
                    Name = tagName,
                    Description = GenerateTagDescription(tagName)
                });
            }
        }
    }

    private static List<FeatureStructure> DiscoverFeatureStructures(Assembly[] assemblies)
    {
        var features = new Dictionary<string, FeatureStructure>();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(EndpointAbstractBase)));
            
            foreach (var type in types)
            {
                var featureInfo = ExtractFeatureFromNamespace(type.Namespace);
                if (featureInfo != null)
                {
                    if (!features.TryGetValue(featureInfo.FeatureName, out var value))
                    {
                        value = new FeatureStructure
                        {
                            FeatureName = featureInfo.FeatureName,
                            OperationTypes = []
                        };
                        features[featureInfo.FeatureName] = value;
                    }

                    if (!string.IsNullOrEmpty(featureInfo.OperationType))
                    {
                        value.OperationTypes.Add(featureInfo.OperationType);
                    }
                }
            }
        }

        return [.. features.Values];
    }

    private static FeatureInfo? ExtractFeatureFromNamespace(string? namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName))
            return null;

        var parts = namespaceName.Split('.');

        // Look for Endpoints pattern: *.Endpoints.{FeatureName}.{OperationType}
        var endpointsIndex = Array.FindIndex(parts, part =>
            (part.Contains("Endpoint", StringComparison.OrdinalIgnoreCase) && !part.Contains("Controller", StringComparison.OrdinalIgnoreCase))
            || (part.Contains("Controller", StringComparison.OrdinalIgnoreCase) && !part.Contains("Endpoint", StringComparison.OrdinalIgnoreCase)));
            

        if (endpointsIndex >= 0 && endpointsIndex + 1 < parts.Length)
        {
            var featureName = parts[endpointsIndex + 1];
            var operationType = endpointsIndex + 2 < parts.Length ? parts[endpointsIndex + 2] : null;

            return new FeatureInfo
            {
                FeatureName = featureName,
                OperationType = operationType
            };
        }

        return null;
    }

    private static string GenerateTagDescription(string tagName) => tagName switch
    {
        var name when name.Contains("Command", StringComparison.OrdinalIgnoreCase) =>
            $"Operations that modify {ExtractFeatureName(name).ToLower()} data (Create, Update, Delete)",
        var name when name.Contains("Quer", StringComparison.OrdinalIgnoreCase) =>
            $"Operations that retrieve {ExtractFeatureName(name).ToLower()} data (Get, List, Search)",
        _ => $"{tagName} related operations"
    };

    private static string ExtractFeatureName(string tagName)
    {
        var parts = tagName.Split(' ');
        return parts.Length > 0 ? parts[0] : tagName;
    }

    private class FeatureStructure
    {
        public string FeatureName { get; set; } = string.Empty;
        public HashSet<string> OperationTypes { get; set; } = [];
    }

    private class FeatureInfo
    {
        public string FeatureName { get; set; } = string.Empty;
        public string? OperationType { get; set; }
    }
}
