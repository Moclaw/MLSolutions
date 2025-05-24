namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using the request headers.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromHeaderAttribute : Attribute 
{
    /// <summary>
    /// Gets or sets the name of the header to bind from. If null, uses the property name.
    /// </summary>
    public string? Name { get; set; }
}
