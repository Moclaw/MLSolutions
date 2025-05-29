using System.ComponentModel;

namespace MinimalAPI;

/// <summary>
/// Configuration options for API versioning
/// </summary>
public class VersioningOptions
{
    /// <summary>
    /// Version prefix (e.g., "v" for routes like "/v1/api/...")
    /// </summary>
    public string Prefix { get; set; } = "v";

    /// <summary>
    /// Default version when none is specified
    /// </summary>
    public int DefaultVersion { get; set; } = 1;

    /// <summary>
    /// List of supported versions
    /// </summary>
    public int[] SupportedVersions { get; set; } = [1];

    /// <summary>
    /// Whether to include version in route template
    /// </summary>
    public bool IncludeVersionInRoute { get; set; } = true;

    /// <summary>
    /// Base route template (e.g., "/api" or "/")
    /// </summary>
    public string BaseRouteTemplate { get; set; } = "/api";

    /// <summary>
    /// How to read version information from requests
    /// </summary>
    public VersionReadingStrategy ReadingStrategy { get; set; } = VersionReadingStrategy.UrlSegment;

    /// <summary>
    /// Query parameter name for version reading
    /// </summary>
    public string QueryParameterName { get; set; } = "version";

    /// <summary>
    /// Header name for version reading
    /// </summary>
    public string VersionHeaderName { get; set; } = "X-API-Version";

    /// <summary>
    /// Media type parameter name for version reading
    /// </summary>
    public string MediaTypeParameterName { get; set; } = "version";

    /// <summary>
    /// Whether to assume default version when unspecified
    /// </summary>
    public bool AssumeDefaultVersionWhenUnspecified { get; set; } = true;

    /// <summary>
    /// Whether to generate Swagger documents for each version
    /// </summary>
    public bool GenerateSwaggerDocs { get; set; } = true;

    /// <summary>
    /// Swagger document title template
    /// </summary>
    public string SwaggerDocTitle { get; set; } = "API";

    /// <summary>
    /// Swagger document description template
    /// </summary>
    public string SwaggerDocDescription { get; set; } = "API Documentation";

    /// <summary>
    /// Generate versioned route template
    /// </summary>
    public string GetVersionedRoute(string route, int? version = null)
    {
        var ver = version ?? DefaultVersion;
        var versionSegment = IncludeVersionInRoute ? $"/{Prefix}{ver}" : "";
        var cleanRoute = route.TrimStart('/');
        var baseRoute = BaseRouteTemplate.TrimEnd('/');
        
        return $"{baseRoute}{versionSegment}/{cleanRoute}";
    }

    /// <summary>
    /// Get version from route template
    /// </summary>
    public int? ExtractVersionFromRoute(string route)
    {
        if (!IncludeVersionInRoute) return null;
        
        var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (segment.StartsWith(Prefix) && 
                int.TryParse(segment.Substring(Prefix.Length), out var version))
            {
                return version;
            }
        }
        return null;
    }

    /// <summary>
    /// Get Swagger document name for version
    /// </summary>
    public string GetSwaggerDocName(int version)
    {
        return $"{Prefix}{version}";
    }

    /// <summary>
    /// Get Swagger document info for version
    /// </summary>
    public (string Name, string Title, string Description) GetSwaggerDocInfo(int version)
    {
        var name = GetSwaggerDocName(version);
        var title = $"{SwaggerDocTitle} {Prefix.ToUpper()}{version}";
        var description = GetVersionDescription(version);
        return (name, title, description);
    }

    /// <summary>
    /// Get version-specific description
    /// </summary>
    public string GetVersionDescription(int version)
    {
        return version switch
        {
            1 => $"{SwaggerDocDescription} - Initial version with core functionality",
            2 => $"{SwaggerDocDescription} - Enhanced version with additional features",
            3 => $"{SwaggerDocDescription} - Advanced version with extended capabilities",
            _ => $"{SwaggerDocDescription} - Version {version}"
        };
    }

    /// <summary>
    /// Get all supported version tabs for SwaggerUI
    /// </summary>
    public IEnumerable<(string DocumentName, string DisplayName, string Description)> GetSwaggerTabs()
    {
        return SupportedVersions.Select(version =>
        {
            var docName = GetSwaggerDocName(version);
            var displayName = $"{SwaggerDocTitle} {Prefix.ToUpper()}{version}";
            var description = GetVersionDescription(version);
            return (docName, displayName, description);
        });
    }
}

/// <summary>
/// Default versioning options implementation
/// </summary>
public class DefaultVersioningOptions : VersioningOptions
{
    public DefaultVersioningOptions()
    {
        Prefix = "v";
        DefaultVersion = 1;
        SupportedVersions = [1, 2];
        IncludeVersionInRoute = true;
        BaseRouteTemplate = "/api";
        ReadingStrategy = VersionReadingStrategy.UrlSegment;
        GenerateSwaggerDocs = true;
        SwaggerDocTitle = "Minimal API";
        SwaggerDocDescription = "RESTful API with versioning support";
    }
}

/// <summary>
/// Version reading strategies
/// </summary>
[Flags]
public enum VersionReadingStrategy
{
    /// <summary>
    /// Read version from query string parameter
    /// </summary>
    [Description("Query parameter")]
    QueryString = 1,

    /// <summary>
    /// Read version from URL segment
    /// </summary>
    [Description("URL segment")]
    UrlSegment = 2,

    /// <summary>
    /// Read version from HTTP header
    /// </summary>
    [Description("HTTP header")]
    Header = 4,

    /// <summary>
    /// Read version from media type parameter
    /// </summary>
    [Description("Media type")]
    MediaType = 8,

    /// <summary>
    /// Combine query string and URL segment
    /// </summary>
    [Description("Query or URL")]
    QueryStringOrUrlSegment = QueryString | UrlSegment,

    /// <summary>
    /// Combine header and URL segment
    /// </summary>
    [Description("Header or URL")]
    HeaderOrUrlSegment = Header | UrlSegment,

    /// <summary>
    /// Combine query string and header
    /// </summary>
    [Description("Query or Header")]
    QueryStringOrHeader = QueryString | Header,

    /// <summary>
    /// Use all strategies
    /// </summary>
    [Description("All methods")]
    All = QueryString | UrlSegment | Header | MediaType
}