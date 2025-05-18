namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies the route template for an endpoint handler
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RouteAttribute(string template) : Attribute
{
    /// <summary>
    /// The route template
    /// </summary>
    public string Template { get; } = template;
}
