namespace MinimalAPI.Attributes;

/// <summary>
/// Attribute to provide summary and description for OpenAPI endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class OpenApiSummaryAttribute(string summary) : Attribute
{
    public string Summary { get; } = summary;
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
}
