using Microsoft.AspNetCore.Http;

namespace MinimalAPI;
public interface IEndpoint
{
    /// <summary>
    /// the http context of the current request
    /// </summary>
    HttpContext HttpContext { get; } //this is for allowing consumers to write extension methods

    /// <summary>
    /// gets the endpoint definition which contains all the configuration info for the endpoint
    /// </summary>
    EndpointDefinition Definition { get; } //also for extensibility
}