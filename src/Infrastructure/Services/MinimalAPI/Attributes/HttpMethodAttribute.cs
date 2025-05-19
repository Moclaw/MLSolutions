namespace MinimalAPI.Attributes;

/// <summary>
/// Base attribute for HTTP method attributes
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public abstract class HttpMethodAttribute : Attribute
{
    /// <summary>
    /// The HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// The route template
    /// </summary>
    public string Route { get; }

    protected HttpMethodAttribute(string method, string route)
    {
        Method = method;
        Route = route;
    }
}
