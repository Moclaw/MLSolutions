using MediatR;
using Microsoft.AspNetCore.Http;
using MinimalAPI.Extensions;
using Shared.Responses;
using System.Text.Json;

namespace MinimalAPI.Endpoints;

public abstract partial class EndpointBase<TRequest, TResponse>(IMediator mediator) : EndpointAbstractBase
    where TRequest : class, new()
    where TResponse : notnull
{
    protected readonly IMediator _mediator = mediator;
    
    public abstract Task<Response<TResponse>> HandleAsync(TRequest req, CancellationToken ct); 
    
    internal override async Task ExecuteAsync(CancellationToken ct)
    {
        // Bind the request from the HTTP context
        TRequest req = default!;
        if (typeof(TRequest) != typeof(object))
        {
            req = await HttpContext.BindAsync<TRequest>(ct);
        }

        var res = await HandleAsync(req, ct);
        var statusCode = 200;
        if (res is IResponse response)
        {
            statusCode = response.StatusCode;
        }
        await SendAsync(res, statusCode, ct);
    }

    private Task SendAsync(
        Response<TResponse> response,
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

public abstract partial class EndpointBase<TRequest>(IMediator mediator) : EndpointAbstractBase
    where TRequest : class, new()
{
    protected readonly IMediator _mediator = mediator;

    public abstract Task<Response> HandleAsync(TRequest req, CancellationToken ct);

    internal override async Task ExecuteAsync(CancellationToken ct)
    {
        // Bind the request from the HTTP context
        TRequest req = default!;
        if (typeof(TRequest) != typeof(object))
        {
            req = await HttpContext.BindAsync<TRequest>(ct);
        }
        var res = await HandleAsync(req, ct);
        var statusCode = 200;
        if (res is IResponse response)
        {
            statusCode = response.StatusCode;
        }
        await SendAsync(res, statusCode, ct);
    }
    private Task SendAsync(
        IResponse response,
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
