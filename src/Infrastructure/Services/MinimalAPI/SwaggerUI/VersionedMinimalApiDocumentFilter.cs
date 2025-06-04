using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MinimalAPI.SwaggerUI;

/// <summary>
/// Document filter for versioned API documentation
/// </summary>
public class VersionedMinimalApiDocumentFilter(SwaggerUIOptions options, IWebHostEnvironment environment, IConfiguration configuration) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var versioning = options.VersioningOptions ?? new DefaultVersioningOptions();
        var currentVersion = ExtractVersionFromDocumentName(context.DocumentName, versioning);

        // Enhance document info for current version
        swaggerDoc.Info.Title = $"{options.Title}";
        swaggerDoc.Info.Version = context.DocumentName;
        swaggerDoc.Info.Description = GetVersionSpecificDescription(currentVersion, options.Description);

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

        // Add servers information
        swaggerDoc.Servers = GetServersFromLaunchSettings();

        // Filter paths for current version only
        FilterPathsForVersion(swaggerDoc, currentVersion, versioning);

        // Add dynamic tags for current version
        AddVersionSpecificTags(swaggerDoc, currentVersion.ToString());

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

    private static int ExtractVersionFromDocumentName(string documentName, VersioningOptions versioning)
    {
        if (documentName.StartsWith(versioning.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var versionString = documentName.Substring(versioning.Prefix.Length);
            if (int.TryParse(versionString, out var version))
            {
                return version;
            }
        }
        return versioning.DefaultVersion;
    }

    private static string GetVersionSpecificDescription(int version, string? baseDescription)
    {
        var versionInfo = version switch
        {
            1 => "Initial API version with core functionality",
            2 => "Enhanced API version with additional features and improvements",
            3 => "Advanced API version with extended capabilities",
            _ => $"API version {version}"
        };

        return string.IsNullOrEmpty(baseDescription) 
            ? versionInfo 
            : $"{baseDescription} - {versionInfo}";
    }

    private void FilterPathsForVersion(OpenApiDocument swaggerDoc, int targetVersion, VersioningOptions versioning)
    {
        var pathsToRemove = new List<string>();

        foreach (var path in swaggerDoc.Paths)
        {
            var extractedVersion = versioning.ExtractVersionFromRoute(path.Key) ?? versioning.DefaultVersion;
            
            if (extractedVersion != targetVersion)
            {
                pathsToRemove.Add(path.Key);
                continue;
            }

            // For paths that belong to this version, ensure proper operation handling
            foreach (var operation in path.Value.Operations.Values)
            {
                if (operation.Tags?.Any() == true)
                {
                    var updatedTags = operation.Tags
                        .Select(tag => new OpenApiTag 
                        { 
                            Name = tag.Name,
                            Description = tag.Description
                        })
                        .ToList();
                    
                    operation.Tags = updatedTags;
                }

                // Ensure operation has proper version-specific operation ID if missing
                if (string.IsNullOrEmpty(operation.OperationId))
                {
                    var pathKey = path.Key.Replace("/", "_").Replace("{", "").Replace("}", "");
                    var httpMethod = path.Value.Operations.FirstOrDefault(kvp => kvp.Value == operation).Key.ToString().ToUpper();
                    operation.OperationId = $"V{targetVersion}_{pathKey}_{httpMethod}";
                }
            }
        }

        foreach (var pathToRemove in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(pathToRemove);
        }
    }

    private List<OpenApiServer> GetServersFromLaunchSettings()
    {
        var servers = new List<OpenApiServer>();

        try
        {
            // Get URLs from configuration
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
            servers.Add(new OpenApiServer 
            { 
                Url = "/", 
                Description = $"Current server - {environment.EnvironmentName}"
            });
        }

        return servers;
    }

    private void AddVersionSpecificTags(OpenApiDocument swaggerDoc, string currentVersion)
    {
        // Extract version number (e.g., "v1" -> "1")
        string versionNumber = currentVersion.TrimStart('v');
        
        // Create or update tags with version information
        if (swaggerDoc.Tags == null) swaggerDoc.Tags = new List<OpenApiTag>();
        
        // Define tag categories and their descriptions
        var tagCategories = new Dictionary<string, string>
        {
            { "S3 Commands", "Operations that modify S3 data (Create, Update, Delete)" },
            { "S3 Queries", "Operations that retrieve S3 data (Get, List, Search)" },
            { "Todos Commands", "Operations that modify todos data (Create, Update, Delete)" },
            { "Todos Queries", "Operations that retrieve todos data (Get, List, Search)" },
            { "Tags Commands", "Operations that modify tags data (Create, Update, Delete)" },
            { "Tags Queries", "Operations that retrieve tags data (Get, List, Search)" },
            { "AutofacDemo Commands", "Operations that demonstrate Autofac capabilities (Commands)" }
        };

        // Add or update tags with version information
        foreach (var category in tagCategories)
        {
            string tagName = category.Key;
            string description = $"V{versionNumber} - {category.Value}";
            
            var existingTag = swaggerDoc.Tags.FirstOrDefault(t => t.Name == tagName);
            if (existingTag != null)
            {
                existingTag.Description = description;
            }
            else
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = tagName,
                    Description = description
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

    private static string GenerateVersionSpecificTagDescription(string tagName, int version) => tagName switch
    {
        var name when name.Contains("Command", StringComparison.OrdinalIgnoreCase) =>
            $"V{version} - Operations that modify {ExtractFeatureName(name).ToLower()} data (Create, Update, Delete)",
        var name when name.Contains("Quer", StringComparison.OrdinalIgnoreCase) =>
            $"V{version} - Operations that retrieve {ExtractFeatureName(name).ToLower()} data (Get, List, Search)",
        _ => $"V{version} - {tagName} related operations"
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
