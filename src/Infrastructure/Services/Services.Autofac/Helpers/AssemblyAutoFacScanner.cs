using Autofac;
using Core.Constants;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Services.Autofac.Helpers
{
    public abstract class AssemblyAutoFacScanner(params Assembly[] assemblies)
    {
        /// <summary>
        /// Finds service types in the specified assemblies
        /// </summary>
        /// <param name="suffixes">Service name suffixes to look for</param>
        /// <returns>A dictionary of service types with their implementation types</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public Dictionary<Type, List<Type>> FindServiceTypes(params string[] suffixes)
        {
            var result = new Dictionary<Type, List<Type>>();

            // If no suffixes are specified, use default ones
            if (suffixes.Length == 0)
            {
                suffixes = AutofacConstants.ServiceConventions.GetDefaultSuffixes();
            }

            // Find all concrete types that end with the specified suffixes
            var implementations = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t is { IsInterface: false, IsAbstract: false }
                    && suffixes.Any(suffix =>
                        t.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                    )
                )
                .ToList();

            foreach (var implementation in implementations)
            {
                // Find interfaces that match the implementation name (e.g., IUserService for UserService)
                var interfaces = implementation
                    .GetInterfaces()
                    .Where(i =>
                        i.Name.Length > 1
                        && i.Name.StartsWith($"I")
                        && implementation.Name.EndsWith(
                            i.Name[1..],
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .ToList();

                // If no matching interfaces, try to find any interface the type implements
                if (interfaces.Count == 0)
                {
                    interfaces = implementation.GetInterfaces().ToList();
                }

                // Add the implementation to the result
                foreach (var interfaceType in interfaces)
                {
                    if (!result.TryGetValue(interfaceType, out var value))
                    {
                        value = [];
                        result[interfaceType] = value;
                    }

                    value.Add(implementation);
                }
            }

            return result;
        }

        /// <summary>
        /// Registers service types with the container builder
        /// </summary>
        /// <param name="containerBuilder">The container builder</param>
        /// <param name="serviceLifetime">The service lifetime</param>
        /// <param name="suffixes">Service name suffixes to look for</param>
        /// <returns>The container builder</returns>
        public ContainerBuilder RegisterServices(
            ContainerBuilder containerBuilder,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            params string[] suffixes
        )
        {
            var serviceTypes = FindServiceTypes(suffixes);

            foreach (var (interfaceType, implementationTypes) in serviceTypes)
            {
                switch (implementationTypes.Count)
                {
                    case 1:
                        {
                            // Register single implementation
                            var registration = containerBuilder
                                .RegisterType(implementationTypes[0])
                                .As(interfaceType);

                            // Set the lifetime
                            switch (serviceLifetime)
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
                                default:
                                    throw new ArgumentOutOfRangeException(
                                        nameof(serviceLifetime),
                                        serviceLifetime,
                                        null
                                    );
                            }

                            break;
                        }
                    case > 1:
                        {
                            // Register multiple implementations
                            foreach (
                                var registration in implementationTypes.Select(implementationType =>
                                    containerBuilder
                                        .RegisterType(implementationType)
                                        .Named(implementationType.Name, interfaceType)
                                )
                            )
                            {
                                // Set the lifetime
                                switch (serviceLifetime)
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
                                    default:
                                        throw new ArgumentOutOfRangeException(
                                            nameof(serviceLifetime),
                                            serviceLifetime,
                                            null
                                        );
                                }
                            }

                            break;
                        }
                }
            }

            return containerBuilder;
        }
    }
}
