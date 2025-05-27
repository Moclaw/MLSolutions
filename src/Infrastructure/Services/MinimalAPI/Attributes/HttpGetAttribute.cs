namespace MinimalAPI.Attributes;

/// <summary>
/// Identifies an endpoint that supports the HTTP GET method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpGetAttribute(string route = "") : HttpMethodAttribute("GET", route)
{
}
