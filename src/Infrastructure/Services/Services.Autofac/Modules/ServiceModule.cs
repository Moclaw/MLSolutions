using Autofac;
using Services.Autofac.Attributes;
using System.Reflection;

namespace Services.Autofac.Modules
{
    public class ServiceModule(
        Assembly[] assemblies,
        bool registerByNamingConvention = true,
        bool autoRegisterConcreteTypes = false
    ) : global::Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register by attributes
            RegisterByAttributes(builder);

            // Register by naming convention if enabled
            if (registerByNamingConvention)
            {
                RegisterByNamingConvention(builder);
            }

            // Auto-register concrete types if enabled
            if (autoRegisterConcreteTypes)
            {
                RegisterConcreteTypes(builder);
            }
        }

        private void RegisterByAttributes(ContainerBuilder builder)
        {
            // Register Transient services
            builder
                .RegisterAssemblyTypes(assemblies)
                .Where(t =>
                    t.GetCustomAttributes(typeof(TransientServiceAttribute), true).Length > 0
                )
                .AsImplementedInterfaces()
                .AsSelf()
                .Named(
                    t =>
                    {
                        var attr = t.GetCustomAttribute<TransientServiceAttribute>();
                        return !string.IsNullOrEmpty(attr?.ServiceName) ? attr.ServiceName : t.Name;
                    },
                    typeof(object)
                )
                .InstancePerDependency();

            // Register Scoped services
            builder
                .RegisterAssemblyTypes(assemblies)
                .Where(t => t.GetCustomAttributes(typeof(ScopedServiceAttribute), true).Length > 0)
                .AsImplementedInterfaces()
                .AsSelf()
                .Named(
                    t =>
                    {
                        var attr = t.GetCustomAttribute<ScopedServiceAttribute>();
                        return !string.IsNullOrEmpty(attr?.ServiceName) ? attr.ServiceName : t.Name;
                    },
                    typeof(object)
                )
                .InstancePerLifetimeScope();

            // Register Singleton services
            builder
                .RegisterAssemblyTypes(assemblies)
                .Where(t =>
                    t.GetCustomAttributes(typeof(SingletonServiceAttribute), true).Length > 0
                )
                .AsImplementedInterfaces()
                .AsSelf()
                .Named(
                    t =>
                    {
                        var attr = t.GetCustomAttribute<SingletonServiceAttribute>();
                        return !string.IsNullOrEmpty(attr?.ServiceName) ? attr.ServiceName : t.Name;
                    },
                    typeof(object)
                )
                .SingleInstance();
        }

        private void RegisterByNamingConvention(ContainerBuilder builder)
        {
            // Register services by naming convention (e.g., IUserService implemented by UserService)
            // This will find all classes that end with specific suffixes and register them with their interfaces
            var serviceTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    (
                        t.Name.EndsWith("Service")
                        || t.Name.EndsWith("Repository")
                        || t.Name.EndsWith("Manager")
                    )
                    && !t.IsInterface
                    && !t.IsAbstract
                )
                .ToList();

            foreach (var serviceType in serviceTypes)
            {
                // Skip types that already have service attributes
                if (
                    serviceType.GetCustomAttributes(typeof(TransientServiceAttribute), true).Length
                        > 0
                    || serviceType.GetCustomAttributes(typeof(ScopedServiceAttribute), true).Length
                        > 0
                    || serviceType
                        .GetCustomAttributes(typeof(SingletonServiceAttribute), true)
                        .Length > 0
                )
                {
                    continue;
                }

                var interfaces = serviceType
                    .GetInterfaces()
                    .Where(i =>
                        i.Name.Contains(
                            serviceType.Name.Replace("Impl", "").Replace("Implementation", "")
                        )
                    )
                    .ToList();

                if (interfaces.Count != 0)
                {
                    // Register with all matching interfaces
                    builder
                        .RegisterType(serviceType)
                        .As(interfaces.ToArray())
                        .InstancePerLifetimeScope(); // Default to scoped lifetime
                }
            }
        }

        private void RegisterConcreteTypes(ContainerBuilder builder) =>
            // Auto-register concrete types not already registered
            builder
                .RegisterAssemblyTypes(assemblies)
                .Where(t =>
                    !t.IsInterface
                    && !t.IsAbstract
                    && t.GetCustomAttributes(typeof(TransientServiceAttribute), true).Length == 0
                    && t.GetCustomAttributes(typeof(ScopedServiceAttribute), true).Length == 0
                    && t.GetCustomAttributes(typeof(SingletonServiceAttribute), true).Length == 0
                )
                .AsSelf()
                .InstancePerDependency();
    }
}
