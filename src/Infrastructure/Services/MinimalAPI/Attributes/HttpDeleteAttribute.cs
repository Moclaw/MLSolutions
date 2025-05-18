namespace MinimalAPI.Attributes;

/// <summary>
/// Identifies an endpoint that supports the HTTP DELETE method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpDeleteAttribute : HttpMethodAttribute
{
    public HttpDeleteAttribute(string route = "") : base("DELETE", route)
    {
    }
}
