using Host.Handlers;

namespace Host.Services;

internal interface IExceptionHandlerFactory
{
    IExceptionHanlder GetInstances(Exception exception);
}
