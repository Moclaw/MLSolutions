using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Host.Handlers;

public interface IExceptionHanlder
{
    Task HandleException(HttpContext httpContext, Exception exception, ILogger logger);
}
