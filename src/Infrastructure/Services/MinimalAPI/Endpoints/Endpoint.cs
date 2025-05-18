using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Shard.Responses;

namespace MinimalAPI.Endpoints;

public abstract partial class Endpoint<TRequest, TResponse> : EndpointBase
    where TRequest : notnull
    where TResponse : notnull
{
    static readonly Type _tRequest = typeof(TRequest);
    static readonly Type _tResponse = typeof(TResponse);
    public abstract Task<TResponse> HandleAsync(TRequest req, CancellationToken ct);

    internal override async Task ExecuteAsync(CancellationToken ct)
    {
        // NOTE: In a real implementation, BindRequestAsync would bind from HttpContext, here we just use default
        TRequest req = default!;
        var res = await HandleAsync(req, ct);
        var statusCode = 200;
        if (res is IResponse response)
        {
            statusCode = response.StatusCode;
        }
        await SendAsync(res, statusCode, ct);
    }

    private Task SendAsync(
        TResponse response,
        int statusCode = 200,
        CancellationToken cancellation = default
    )
    {
        HttpContext.Response.StatusCode = statusCode;
        HttpContext.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(response);
        return HttpContext.Response.WriteAsync(json, cancellation);
    }
}
