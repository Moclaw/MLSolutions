namespace MinimalAPI.Attributes;
/// <summary>
/// Identifies an endpoint that supports the HTTP PUT method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HttpPutAttribute(string route = "") : HttpMethodAttribute("PUT", route)
{
}