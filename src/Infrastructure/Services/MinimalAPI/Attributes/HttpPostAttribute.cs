namespace MinimalAPI.Attributes;

/// <summary>
/// Identifies an endpoint that supports the HTTP POST method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute(string route = "") : base("POST", route)
    {
    }
}
