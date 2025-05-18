namespace MinimalAPI.Attributes;

/// <summary>
/// Identifies an endpoint that supports the HTTP GET method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute(string route = "") : base("GET", route)
    {
    }
}
