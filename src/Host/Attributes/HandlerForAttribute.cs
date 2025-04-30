namespace Host.Attributes;
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
internal sealed class HandlerForAttribute(Type exceptionType) : Attribute
{
    public Type ExceptionType { get; } = exceptionType;
}
