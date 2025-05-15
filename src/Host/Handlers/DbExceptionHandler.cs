using System.Data.Common;
using System.Net;
using System.Text.Json;
using Host.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MLSolutions;

namespace Host.Handlers;
[HandlerFor(typeof(DbException))]
internal class DbExceptionHandler : IExceptionHanlder
{
    public Task HandleException(HttpContext httpContext, Exception exception, ILogger logger)
    {
        var exceptionType = exception as DbException;

        var parameters = exceptionType
            ?.BatchCommand?.Parameters.Cast<DbParameter>()
            .Select(p => new { p.ParameterName, p.Value })
            .ToList();

        logger.LogError(
            "Query: {BatchCommandCommandText} - Params: {Serialize}", exceptionType?.BatchCommand?.CommandText, parameters?.Count > 0 ? JsonSerializer.Serialize(parameters) : string.Empty);

        return httpContext.WriteExceptionResult(
            (int)HttpStatusCode.InternalServerError,
            exception.Message
        );
    }
}