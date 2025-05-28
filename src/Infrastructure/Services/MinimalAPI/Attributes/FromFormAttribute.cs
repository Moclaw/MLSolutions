namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies that a parameter or property should be bound using form data from the request body
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class FromFormAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the form field to bind to
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Initializes a new instance of FromFormAttribute
    /// </summary>
    public FromFormAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of FromFormAttribute with the specified name
    /// </summary>
    /// <param name="name">The name of the form field to bind to</param>
    public FromFormAttribute(string name)
    {
        Name = name;
    }
}
