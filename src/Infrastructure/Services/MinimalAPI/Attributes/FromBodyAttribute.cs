namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using the request body.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromBodyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the JSON property to bind from. If null, binds the entire body to the property.
    /// </summary>
    public string? Name { get; set; }
}
