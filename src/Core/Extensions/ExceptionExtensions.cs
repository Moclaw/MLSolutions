using Microsoft.AspNetCore.Http;
using Shared.Responses;

namespace MLSolutions;

public static class ExceptionExtensions
{
    public static Task WriteExceptionResult(
        this HttpContext httpContext,
        int errorCode,
        string errorMessage
    )
    {
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.StatusCode = errorCode;
        }
        var result = new Response(IsSuccess: false, StatusCode: errorCode, Message: errorMessage);

        return httpContext.Response.WriteAsync(result.Serialize());
    }
}
