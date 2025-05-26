namespace MinimalAPI.Attributes;

/// <summary>
/// Attribute to provide summary and description for endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class EndpointSummaryAttribute : Attribute
{
    public string Summary { get; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }

    public EndpointSummaryAttribute(string summary)
    {
        Summary = summary;
    }
}
