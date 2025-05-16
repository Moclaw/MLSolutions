using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Services.Autofac.Modules;

namespace Services.Autofac.Extensions
{
    /// <summary>
    /// Extensions for easily configuring Autofac in ASP.NET Core applications with WebApplicationBuilder
    /// </summary>
    public static class WebApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds Autofac as the service provider factory for the application
        /// </summary>
        /// <param name="builder">The web application builder</param>
        /// <param name="configureContainer">Action to configure the container</param>
        /// <returns>The web application builder</returns>
        public static WebApplicationBuilder UseAutofac(
            this WebApplicationBuilder builder,
            Action<ContainerBuilder>? configureContainer = null
        )
        {
            builder.Host.UseServiceProviderFactory(
                new AutofacServiceProviderFactory(containerBuilder =>
                {
                    configureContainer?.Invoke(containerBuilder);
                })
            );

            return builder;
        }

        /// <summary>
        /// Adds Autofac as the service provider factory for the application and registers services by convention
        /// </summary>
        /// <param name="builder">The web application builder</param>
        /// <param name="assemblies">The assemblies to scan for services</param>
        /// <returns>The web application builder</returns>
        public static WebApplicationBuilder UseAutofacWithConventions(
            this WebApplicationBuilder builder,
            params Assembly[] assemblies
        )
        {
            builder.Host.UseServiceProviderFactory(
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

            return builder;
        }

        /// <summary>
        /// Adds a specific module to the Autofac container
        /// </summary>
        /// <typeparam name="TModule">The type of the module to add</typeparam>
        /// <param name="builder">The web application builder</param>        /// <param name="module">The module instance to add</param>
        /// <returns>The web application builder</returns>
        public static WebApplicationBuilder AddAutofacModule<TModule>(
            this WebApplicationBuilder builder,
            TModule module
        )
            where TModule : global::Autofac.Module
        {
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(module);
            });

            return builder;
        }
    }
}
