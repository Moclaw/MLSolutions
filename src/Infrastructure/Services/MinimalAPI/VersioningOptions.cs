namespace MinimalAPI;
public abstract class VersioningOptions
{
    /// <summary>
    /// the prefix used in front of the version (for example 'v' produces 'v{version}').
    /// </summary>
    public string? Prefix { internal get; set; } = "v";

    /// <summary>
    /// this value will be used on endpoints that does not specify a version
    /// </summary>
    public int DefaultVersion { internal get; set; } = 1;

    /// <summary>
    /// set to true if you'd like to prefix the version to the route instead of being suffixed which is the default
    /// </summary>
    public bool? PrependToRoute { internal get; set; }

    public override string ToString()
    {
        return string.Join(string.Empty, [Prefix, DefaultVersion]);
    }
}

/// <summary>
/// Default implementation of VersioningOptions
/// </summary>
public class DefaultVersioningOptions : VersioningOptions
{
}