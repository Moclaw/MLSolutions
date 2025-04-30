using System.ComponentModel.DataAnnotations;
using Host.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Host.Middleware;

internal record ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IExceptionHandlerFactory _exceptionHandlerFactory;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IExceptionHandlerFactory exceptionHandlerFactory
    )
    {
        _next = next;
        _logger = logger;
        _exceptionHandlerFactory = exceptionHandlerFactory;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            if (ex is not ValidationException)
            {
                _logger.LogError(
                    ex,
                    "Error HResult: {ExHResult} - Error Message: {ExMessage}",
                    ex.HResult,
                    ex.Message
                );
            }

            var exceptionHandler = _exceptionHandlerFactory.GetInstances(ex);
            await exceptionHandler.HandleException(httpContext, ex, _logger);
        }
    }
}
