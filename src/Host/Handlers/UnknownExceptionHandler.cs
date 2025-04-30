using System.Net;
using Host.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MLSolutions;
using Shard.Responses;

namespace Host.Handlers;
[HandlerFor(typeof(Exception))]
internal class UnknownExceptionHandler : IExceptionHanlder
{
    public Task HandleException(HttpContext httpContext, Exception exception, ILogger logger)
    {
        logger.LogError(exception, "Unknown exception occurred");
        return httpContext.WriteExceptionResult(
            (int)HttpStatusCode.BadRequest,
            exception?.Message ?? "Please contact support for more information."
        );
    }
}