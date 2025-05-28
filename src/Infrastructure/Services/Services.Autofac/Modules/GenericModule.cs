using Autofac;
using Autofac.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Services.Autofac.Modules
{
    public class GenericModule(
        Assembly[] assemblies,
        ServiceLifetime defaultLifetime = ServiceLifetime.Scoped
    ) : global::Autofac.Module
    {
        private readonly List<GenericRegistration> _registrations = [];

        public GenericModule RegisterGeneric(
            Type openGenericServiceType,
            Type openGenericImplementationType,
            ServiceLifetime lifetime = ServiceLifetime.Scoped
        )
        {
            _registrations.Add(
                new GenericRegistration
                {
                    OpenGenericServiceType = openGenericServiceType,
                    OpenGenericImplementationType = openGenericImplementationType,
                    Lifetime = lifetime,
                }
            );

            return this;
        }

        public GenericModule RegisterGeneric<TService, TImplementation>(
            ServiceLifetime lifetime = ServiceLifetime.Scoped
        ) => RegisterGeneric(typeof(TService), typeof(TImplementation), lifetime);

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var registration in _registrations)
            {
                var registrationBuilder = builder
                    .RegisterGeneric(registration.OpenGenericImplementationType)
                    .As(registration.OpenGenericServiceType);

                ApplyLifetime(registrationBuilder, registration.Lifetime);
            }

            // Auto register concrete implementations of open generic interfaces
            var allTypes = assemblies.SelectMany(a => a.GetTypes()).ToList();
            var openGenericInterfaces = allTypes
                .Where(t => t.IsGenericTypeDefinition && t.IsInterface)
                .ToList();

            foreach (var openGenericInterface in openGenericInterfaces)
            {
                // Find all concrete types that implement this open generic interface
                var implementations = allTypes
                    .Where(t =>
                        t.IsClass
                        && !t.IsAbstract
                        && !t.IsGenericTypeDefinition
                        && t.GetInterfaces()
                            .Any(i =>
                                i.IsGenericType
                                && i.GetGenericTypeDefinition() == openGenericInterface
                            )
                    )
                    .ToList();

                foreach (var implementation in implementations)
                {
                    // Find the closed generic interface that this type implements
                    var closedInterface = implementation
                        .GetInterfaces()
                        .FirstOrDefault(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface
                        );

                    if (closedInterface != null)
                    {
                        var registrationBuilder = builder
                            .RegisterType(implementation)
                            .As(closedInterface);

                        ApplyLifetime(registrationBuilder, defaultLifetime);
                    }
                }
            }
        }

        private static void ApplyLifetime<TLimit, TActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder,
            ServiceLifetime lifetime
        )
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case ServiceLifetime.Scoped:
                    registrationBuilder.InstancePerLifetimeScope();
                    break;
                case ServiceLifetime.Transient:
                    registrationBuilder.InstancePerDependency();
                    break;
            }
        }

        private class GenericRegistration
        {
            public required Type OpenGenericServiceType { get; set; }
            public required Type OpenGenericImplementationType { get; set; }
            public ServiceLifetime Lifetime { get; set; }
        }
    }
}
