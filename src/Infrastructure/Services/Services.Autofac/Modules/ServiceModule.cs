using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Services.Autofac.Modules
{
    /// <summary>
    /// Module for registering services
    /// </summary>
    public class ServiceModule : BaseModule
    {
        private readonly Microsoft.Extensions.DependencyInjection.ServiceLifetime _lifetime;

        /// <summary>
        /// Creates a new service module
        /// </summary>
        /// <param name="lifetime">The lifetime for services (default: scoped)</param>
        /// <param name="assemblies">The assemblies to scan for services</param>
        public ServiceModule(
            Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
            params Assembly[] assemblies) : base(assemblies)
        {
            _lifetime = lifetime;
        }

        /// <summary>
        /// Load the service registrations
        /// </summary>
        /// <param name="builder">The container builder</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Register all service types by convention
            foreach (var assembly in Assemblies)
            {
                var registration = builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Service") && !t.IsInterface && !t.IsAbstract)
                    .AsImplementedInterfaces();

                ApplyLifetime(registration, _lifetime);
            }
        }
    }
}