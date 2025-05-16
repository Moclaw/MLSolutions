using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Services.Autofac
{
    public static class ServiceRegister
    {
        /// <summary>
        /// Adds Autofac as the service provider factory and registers modules
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureContainer">Action to configure the container</param>
        /// <returns>AutofacServiceProviderFactory</returns>
        public static AutofacServiceProviderFactory AddAutofacServiceProviderFactory(
            this IServiceCollection services,
            Action<ContainerBuilder>? configureContainer = null)
        {
            var factory = new AutofacServiceProviderFactory(builder =>
            {
                configureContainer?.Invoke(builder);
            });

            return factory;
        }

        /// <summary>
        /// Registers all modules in the specified assemblies
        /// </summary>
        /// <param name="builder">The container builder</param>
        /// <param name="assemblies">The assemblies to scan for modules</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder RegisterModules(
            this ContainerBuilder builder,
            params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                // Register all types that implement IModule in the assembly
                builder.RegisterAssemblyModules(assembly);
            }

            return builder;
        }

        /// <summary>
        /// Registers all types that implement TService as the service type
        /// </summary>
        /// <typeparam name="TService">The service type to register</typeparam>
        /// <param name="builder">The container builder</param>
        /// <param name="assemblies">The assemblies to scan for implementations</param>
        /// <param name="lifetime">The lifetime of the registrations</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder RegisterAssemblyTypes<TService>(
            this ContainerBuilder builder,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            params Assembly[] assemblies)
        {
            var registration = builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(TService).IsAssignableFrom(t) && !t.IsAbstract)
                .AsImplementedInterfaces();

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    registration.SingleInstance();
                    break;
                case ServiceLifetime.Scoped:
                    registration.InstancePerLifetimeScope();
                    break;
                case ServiceLifetime.Transient:
                    registration.InstancePerDependency();
                    break;
            }

            return builder;
        }

        /// <summary>
        /// Registers all services with their implementations by convention
        /// </summary>
        /// <param name="builder">The container builder</param>
        /// <param name="assemblies">The assemblies to scan for services</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder RegisterConventions(
            this ContainerBuilder builder,
            params Assembly[] assemblies)
        {
            // Register classes that follow the IService/Service naming convention
            foreach (var assembly in assemblies)
            {
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Service") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                // Register classes that follow the IRepository/Repository naming convention
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Repository") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                // Register other common patterns
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Manager") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Factory") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Provider") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }

            return builder;
        }

        /// <summary>
        /// Creates a module that registers services based on attributes
        /// </summary>
        /// <param name="builder">The container builder</param>
        /// <param name="assemblies">The assemblies to scan for modules</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder ScanForAttributeRegistrations(
            this ContainerBuilder builder,
            params Assembly[] assemblies)
        {
            // This allows scanning assemblies for types with registration attributes
            foreach (var assembly in assemblies)
            {
                // Example: Register all types with a custom [RegisterAs] attribute
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    // Look for any custom attributes that might indicate registration
                    if (type.GetCustomAttribute<SingletonAttribute>() != null)
                    {
                        RegisterByAttribute(builder, type, ServiceLifetime.Singleton);
                    }
                    else if (type.GetCustomAttribute<ScopedAttribute>() != null)
                    {
                        RegisterByAttribute(builder, type, ServiceLifetime.Scoped);
                    }
                    else if (type.GetCustomAttribute<TransientAttribute>() != null)
                    {
                        RegisterByAttribute(builder, type, ServiceLifetime.Transient);
                    }
                }
            }

            return builder;
        }

        private static void RegisterByAttribute(ContainerBuilder builder, Type type, ServiceLifetime lifetime)
        {
            // Register the type with its interfaces
            var registration = builder.RegisterType(type).AsImplementedInterfaces();

            // Set the appropriate lifetime
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    registration.SingleInstance();
                    break;
                case ServiceLifetime.Scoped:
                    registration.InstancePerLifetimeScope();
                    break;
                case ServiceLifetime.Transient:
                    registration.InstancePerDependency();
                    break;
            }
        }
    }

    // Define custom attributes for service lifetime registration
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class ScopedAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class TransientAttribute : Attribute { }
}