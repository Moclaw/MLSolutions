using MediatR;
using Microsoft.AspNetCore.Http;

namespace MinimalAPI.Endpoints;

/// <summary>
/// Base class for API endpoints that use MediatR.
/// </summary>
public abstract class EndpointBase : IEndpoint
{
    public HttpContext HttpContext { get; internal set; } = default!;

    public EndpointDefinition Definition { get; internal set; } = default!;

    internal abstract Task ExecuteAsync(CancellationToken ct);
}
