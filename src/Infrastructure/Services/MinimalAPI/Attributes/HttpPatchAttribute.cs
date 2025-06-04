namespace MinimalAPI.Attributes
{
    /// <summary>
    /// Identifies an endpoint that supports the HTTP PATCH method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPatchAttribute(string route = "") : HttpMethodAttribute("PATCH", route)
    {
    }

}
