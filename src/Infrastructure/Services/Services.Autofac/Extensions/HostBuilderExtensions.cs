using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Autofac.Modules;
using System.Reflection;

namespace Services.Autofac.Extensions
{
    /// <summary>
    /// Extensions for easily configuring Autofac in Generic Host applications
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Adds Autofac as the service provider factory for the host
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureContainer">Action to configure the container</param>
        /// <returns>The host builder</returns>
        public static IHostBuilder UseAutofac(
            this IHostBuilder hostBuilder,
            Action<ContainerBuilder>? configureContainer = null
        )
        {
            hostBuilder.UseServiceProviderFactory(
                new AutofacServiceProviderFactory(containerBuilder =>
                {
                    configureContainer?.Invoke(containerBuilder);
                })
            );

            return hostBuilder;
        }

        /// <summary>
        /// Adds Autofac as the service provider factory for the host and registers services by convention
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="assemblies">The assemblies to scan for services</param>
        /// <returns>The host builder</returns>
        public static IHostBuilder UseAutofacWithConventions(
            this IHostBuilder hostBuilder,
            params Assembly[] assemblies
        )
        {
            hostBuilder.UseServiceProviderFactory(
                new AutofacServiceProviderFactory(containerBuilder =>
                {
                    // Register services by convention
                    containerBuilder.RegisterConventions(assemblies);

                    // Register repositories
                    containerBuilder.RegisterModule(new RepositoryModule(assemblies: assemblies));

                    // Register services
                    containerBuilder.RegisterModule(new ServiceModule(assemblies: assemblies));

                    // Register services based on attributes
                    containerBuilder.RegisterModule(new AttributeModule(assemblies));
                })
            );

            return hostBuilder;
        }

        /// <summary>
        /// Adds a specific module to the Autofac container
        /// </summary>
        /// <typeparam name="TModule">The type of the module to add</typeparam>
        /// <param name="hostBuilder">The host builder</param>        /// <param name="module">The module instance to add</param>
        /// <returns>The host builder</returns>
        public static IHostBuilder AddAutofacModule<TModule>(
            this IHostBuilder hostBuilder,
            TModule module
        )
            where TModule : global::Autofac.Module
        {
            hostBuilder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(module);
            });

            return hostBuilder;
        }
    }
}
