namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using route-data from the current request.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromRouteAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the route parameter to bind from. If null, uses the property name.
    /// </summary>
    public string? Name { get; set; }
}
