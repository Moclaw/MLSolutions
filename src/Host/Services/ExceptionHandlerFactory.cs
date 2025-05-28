using Host.Attributes;
using Host.Handlers;
using Host.Services;
using System.Reflection;
using System.Text.Json;

namespace Host.Middleware;

internal class ExceptionHandlerFactory : IExceptionHandlerFactory
{
    private readonly Dictionary<Type, IExceptionHanlder> _handlers;

    public ExceptionHandlerFactory()
    {
        _handlers = LoadHandlers();
    }

    public IExceptionHanlder GetInstances(Exception exception)
    {
        var exceptionType = exception.GetType();
        if (exceptionType.Name.Equals("JsonReaderException", StringComparison.OrdinalIgnoreCase))
        {
            exceptionType = typeof(JsonException);
        }

        return _handlers.TryGetValue(exceptionType, out var handler)
            ? handler
            : new UnknownExceptionHandler();
    }

    private static Dictionary<Type, IExceptionHanlder> LoadHandlers() =>
        Assembly
            .GetAssembly(typeof(IExceptionHanlder))!
            .GetTypes()
            .Where(t => typeof(IExceptionHanlder).IsAssignableFrom(t) && !t.IsInterface)
            .SelectMany(
                t => t.GetCustomAttributes<HandlerForAttribute>(false),
                (t, attr) => new { Handler = CreateHandlerInstance(t), attr.ExceptionType }
            )
            .ToDictionary(k => k.ExceptionType, v => v.Handler);

    private static IExceptionHanlder CreateHandlerInstance(Type type) => Activator.CreateInstance(type) as IExceptionHanlder ?? new UnknownExceptionHandler();
}
