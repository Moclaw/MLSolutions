using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MLSolutions;
using System.Net;
using System.Text.Json;

namespace Host.Handlers;

internal class JsonExceptionHandler : IExceptionHanlder
{
    public Task HandleException(HttpContext httpContext, Exception exception, ILogger logger)
    {
        if (exception is JsonException jsonEx)
        {
            logger.LogError(
                jsonEx,
                "Error HResult: {ExHResult} - Error Message: {ExMessage}",
                jsonEx.HResult,
                jsonEx.Message
            );
        }

        return httpContext.WriteExceptionResult(
            (int)HttpStatusCode.BadRequest,
            $"Json data is invalid. Cannot Deserialize to generic type."
        );
    }
}
