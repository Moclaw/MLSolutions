namespace MinimalAPI.Attributes;

/// <summary>
/// Attribute to provide summary and description for OpenAPI endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class OpenApiSummaryAttribute : Attribute
{
    public string Summary { get; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }

    public OpenApiSummaryAttribute(string summary)
    {
        Summary = summary;
    }
}
