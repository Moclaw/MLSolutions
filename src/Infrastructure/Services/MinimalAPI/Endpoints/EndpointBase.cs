using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using MinimalAPI.Endpoints;
using Shared.Responses;

/// <summary>
/// Base class for endpoints with request and generic response
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response item type</typeparam>
/// <typeparam name="TResponseWrapper">The response wrapper type (Response or ResponesCollection)</typeparam>
public abstract class EndpointBase<TRequest, TResponse, TResponseWrapper>(IMediator mediator)
    : EndpointAbstractBase
    where TRequest : class, new()
    where TResponse : notnull
    where TResponseWrapper : IResponse
{
    /// <summary>
    /// Mediator instance
    /// </summary>
    protected readonly IMediator _mediator = mediator;

    /// <summary>
    /// Handle the request and return a response
    /// </summary>
    /// <param name="req">The request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response with item(s)</returns>
    public abstract Task<TResponseWrapper> HandleAsync(TRequest req, CancellationToken ct);

    internal override async Task ExecuteAsync(CancellationToken ct)
    {
        // Bind the request from the HTTP context
        TRequest req = default!;
        if (typeof(TRequest) != typeof(object))
        {
            req = await EndpointBindingHelper.BindAsync<TRequest>(HttpContext, ct);
        }

        var res = await HandleAsync(req, ct);
        var statusCode = res.StatusCode;
        await SendAsync(res, statusCode, ct);
    }

    private Task SendAsync(
        TResponseWrapper response,
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

// Type aliases for backward compatibility
/// <summary>
/// Base class for endpoints with request and single response
/// </summary>
public abstract class SingleEndpointBase<TRequest, TResponse>(IMediator mediator)
    : EndpointBase<TRequest, TResponse, Response<TResponse>>(mediator)
    where TRequest : class, new()
    where TResponse : notnull { }

/// <summary>
/// Base class for endpoints with request and no typed response
/// </summary>
public abstract class SingleEndpointBase<TRequest>(IMediator mediator)
    : EndpointBase<TRequest, object, Response>(mediator)
    where TRequest : class, new() { }

/// <summary>
/// Base class for endpoints with request and collection response
/// </summary>
public abstract class CollectionEndpointBase<TRequest, TResponse>(IMediator mediator)
    : EndpointBase<TRequest, TResponse, ResponesCollection<TResponse>>(mediator)
    where TRequest : class, new()
    where TResponse : notnull { }
