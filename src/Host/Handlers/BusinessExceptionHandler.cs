using Host.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MLSolutions;
using Shard.Exceptions;
using System.Net;

namespace Host.Handlers;

[HandlerFor(typeof(BusinessException))]
internal class BusinessExceptionHandler : IExceptionHanlder
{
    public Task HandleException(HttpContext httpContext, Exception exception, ILogger logger)
    {
        var exceptionType = exception as BusinessException;
        return httpContext.WriteExceptionResult(
            (int)HttpStatusCode.BadRequest,
            exceptionType?.Message ?? "Business exception occurred"
        );
    }
}
