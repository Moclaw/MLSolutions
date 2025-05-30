namespace MinimalAPI.Attributes;

/// <summary>
/// Attribute to document API responses in OpenAPI specification
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class OpenApiResponseAttribute(int statusCode) : Attribute
{
    public int StatusCode { get; } = statusCode;
    public Type? ResponseType { get; set; }
    public string? Description { get; set; }
    public string? ContentType { get; set; } = "application/json";
}
