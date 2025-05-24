namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using form data in the request body.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromFormAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the form field to bind from. If null, uses the property name.
    /// </summary>
    public string? Name { get; set; }
}
