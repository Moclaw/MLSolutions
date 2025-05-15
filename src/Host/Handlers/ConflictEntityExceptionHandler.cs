using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MLSolutions;
using Shard.Exceptions;

namespace Host.Handlers;

internal class ConflictEntityExceptionHandler : IExceptionHanlder
{
    public Task HandleException(HttpContext httpContext, Exception exception, ILogger logger)
    {
        var exceptionType = exception as ConflictEntityException;
        return httpContext.WriteExceptionResult(
            StatusCodes.Status409Conflict,
            exceptionType?.Message ?? "Conflict entity exception occurred"
        );
    }
}
