namespace MinimalAPI.Attributes;

/// <summary>
/// Identifies an endpoint that supports the HTTP DELETE method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpDeleteAttribute(string route = "") : HttpMethodAttribute("DELETE", route)
{
}
