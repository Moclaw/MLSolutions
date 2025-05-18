namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies the response type and status code for an endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ProducesAttribute : Attribute
{
    /// <summary>
    /// The status code for the response
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// The content type for the response
    /// </summary>
    public string ContentType { get; }

    public ProducesAttribute(int statusCode = 200, string contentType = "application/json")
    {
        StatusCode = statusCode;
        ContentType = contentType;
    }
}
