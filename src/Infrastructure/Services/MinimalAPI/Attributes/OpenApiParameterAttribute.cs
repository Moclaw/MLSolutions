namespace MinimalAPI.Attributes;

/// <summary>
/// Attribute to document API parameters in OpenAPI specification
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class OpenApiParameterAttribute : Attribute
{
    public string Name { get; }
    public Type Type { get; }
    public string? Description { get; set; }
    public bool Required { get; set; } = false;
    public ParameterLocation Location { get; set; } = ParameterLocation.Auto;
    public object? Example { get; set; }
    public string? Format { get; set; }

    public OpenApiParameterAttribute(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}

public enum ParameterLocation
{
    Auto,    // Will be determined automatically based on request type
    Query,
    Path,
    Header,
    Cookie,
    Body
}
