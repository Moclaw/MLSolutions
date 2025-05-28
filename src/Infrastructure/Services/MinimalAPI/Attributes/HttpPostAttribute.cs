namespace MinimalAPI.Attributes;

/// <summary>
/// Identifies an endpoint that supports the HTTP POST method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpPostAttribute(string route = "") : HttpMethodAttribute("POST", route)
{
}
