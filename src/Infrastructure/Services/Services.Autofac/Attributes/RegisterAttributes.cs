// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Services.Autofac.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TransientServiceAttribute : Attribute
    {
        public string? ServiceName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ScopedServiceAttribute : Attribute
    {
        public string? ServiceName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SingletonServiceAttribute : Attribute
    {
        public string? ServiceName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ServiceContractAttribute : Attribute { }
}
