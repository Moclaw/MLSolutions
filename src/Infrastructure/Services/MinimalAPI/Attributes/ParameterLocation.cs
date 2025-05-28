namespace MinimalAPI.Attributes;

/// <summary>
/// Specifies the location of a parameter in an HTTP request
/// </summary>
public enum ParameterLocation
{
    /// <summary>
    /// Automatically determine the parameter location based on the request type and property characteristics
    /// </summary>
    Auto,

    /// <summary>
    /// Parameter is in the query string
    /// </summary>
    Query,

    /// <summary>
    /// Parameter is in the URL path
    /// </summary>
    Path,

    /// <summary>
    /// Parameter is in the request headers
    /// </summary>
    Header,

    /// <summary>
    /// Parameter is in the request body
    /// </summary>
    Body,

    /// <summary>
    /// Parameter is in form data
    /// </summary>
    Form,

    /// <summary>
    /// Parameter is in a cookie
    /// </summary>
    Cookie
}
