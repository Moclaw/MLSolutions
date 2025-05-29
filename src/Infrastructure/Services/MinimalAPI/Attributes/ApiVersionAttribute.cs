namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies the API version for an endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiVersionAttribute : Attribute
{
    /// <summary>
    /// The API version number
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Optional version description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this version is deprecated
    /// </summary>
    public bool IsDeprecated { get; set; }

    /// <summary>
    /// Initialize with version number
    /// </summary>
    /// <param name="version">Version number (e.g., 1, 2, 3)</param>
    public ApiVersionAttribute(int version)
    {
        Version = version;
    }

    /// <summary>
    /// Initialize with version string (e.g., "1.0", "2.1")
    /// </summary>
    /// <param name="version">Version string</param>
    public ApiVersionAttribute(string version)
    {
        if (decimal.TryParse(version, out var versionNumber))
        {
            Version = (int)versionNumber;
        }
        else
        {
            throw new ArgumentException($"Invalid version format: {version}", nameof(version));
        }
    }
}
