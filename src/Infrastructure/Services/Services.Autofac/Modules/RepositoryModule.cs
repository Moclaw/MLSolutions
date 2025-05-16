using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Services.Autofac.Modules
{
    /// <summary>
    /// Module for registering repositories
    /// </summary>
    public class RepositoryModule : BaseModule
    {
        private readonly Microsoft.Extensions.DependencyInjection.ServiceLifetime _lifetime;

        /// <summary>
        /// Creates a new repository module
        /// </summary>
        /// <param name="lifetime">The lifetime for repositories (default: scoped)</param>
        /// <param name="assemblies">The assemblies to scan for repositories</param>
        public RepositoryModule(
            Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime =
                Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
            params Assembly[] assemblies
        )
            : base(assemblies)
        {
            _lifetime = lifetime;
        }

        /// <summary>
        /// Load the repository registrations
        /// </summary>
        /// <param name="builder">The container builder</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Register all repository types by convention
            foreach (var assembly in Assemblies)
            {
                var registration = builder
                    .RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Repository") && !t.IsInterface && !t.IsAbstract)
                    .AsImplementedInterfaces();

                ApplyLifetime(registration, _lifetime);
            }
        }
    }
}
