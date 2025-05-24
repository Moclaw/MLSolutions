namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using the query string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromQueryAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the query parameter to bind from. If null, uses the property name.
    /// </summary>
    public string? Name { get; set; }
}
