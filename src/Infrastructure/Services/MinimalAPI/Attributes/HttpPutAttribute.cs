namespace MinimalAPI.Attributes;

/// <summary>
/// Identifies an endpoint that supports the HTTP PUT method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute(string route = "") : base("PUT", route)
    {
    }
}
