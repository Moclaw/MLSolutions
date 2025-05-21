using Host.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MLSolutions;
using Shared.Exceptions;
using System.Net;

namespace Host.Handlers;
[HandlerFor(typeof(EntityNotFoundException))]
internal class EntityNotFoundExceptionHandler : IExceptionHanlder
{
    public Task HandleException(HttpContext httpContext, Exception exception, ILogger logger)
    {
        var exceptionType = exception as EntityNotFoundException;

        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        return httpContext.WriteExceptionResult(
            (int)HttpStatusCode.NoContent,
            exceptionType?.Message ?? "Entity not found"
        );
    }
}