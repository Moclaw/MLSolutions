using System.Net;
using Host.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MLSolutions;

namespace Host.Handlers;
[HandlerFor(typeof(UnauthorizedAccessException))]
internal class UnauthorizedExceptionHandler : IExceptionHanlder
{
    public Task HandleException(HttpContext httpContext, Exception exception, ILogger logger) =>
        httpContext.WriteExceptionResult((int)HttpStatusCode.Unauthorized, exception.Message);
}