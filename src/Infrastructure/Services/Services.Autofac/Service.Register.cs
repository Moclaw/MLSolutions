using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Autofac.Extensions;
using Services.Autofac.Modules;
using System.Reflection;

namespace Services.Autofac
{
    public static partial class Register
    {
        /// <summary>
        /// Configure Autofac as the service provider for ASP.NET Core applications
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="configureContainer">Optional delegate to configure the container</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The WebApplicationBuilder</returns>
        public static WebApplicationBuilder AddAutofacServiceProvider(
            this WebApplicationBuilder builder,
            Action<ContainerBuilder>? configureContainer = null,
            params Assembly[] assemblies
        )
        {
            return builder.UseAutofacServiceProvider(configureContainer, assemblies);
        }

        /// <summary>
        /// Configure Autofac as the service provider for Generic Host applications
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureContainer">Optional delegate to configure the container</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The host builder</returns>
        public static IHostBuilder AddAutofacServiceProvider(
            this IHostBuilder hostBuilder,
            Action<ContainerBuilder>? configureContainer = null,
            params Assembly[] assemblies
        )
        {
            return hostBuilder.UseAutofacServiceProvider(configureContainer, assemblies);
        }

        /// <summary>
        /// Register Autofac services to an existing service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureContainer">Optional delegate to configure the container</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The Autofac service provider</returns>
        public static AutofacServiceProvider AddAutofacServices(
            this IServiceCollection services,
            Action<ContainerBuilder>? configureContainer = null,
            params Assembly[] assemblies
        )
        {
            return services.UseAutofacServiceProvider(configureContainer, assemblies);
        }

        /// <summary>
        /// Register assembly-based services with Autofac
        /// </summary>
        /// <param name="containerBuilder">The container builder</param>
        /// <param name="registerByNamingConvention">Whether to register services by naming convention</param>
        /// <param name="autoRegisterConcreteTypes">Whether to auto-register concrete types</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder AddServiceAssemblies(
            this ContainerBuilder containerBuilder,
            bool registerByNamingConvention = true,
            bool autoRegisterConcreteTypes = false,
            params Assembly[] assemblies
        )
        {
            return containerBuilder.RegisterServiceAssemblies(
                registerByNamingConvention, 
                autoRegisterConcreteTypes, 
                assemblies
            );
        }

        /// <summary>
        /// Register generic service types with Autofac
        /// </summary>
        /// <param name="containerBuilder">The container builder</param>
        /// <param name="assemblies">Assemblies to scan for generic services</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder AddGenericServices(
            this ContainerBuilder containerBuilder,
            params Assembly[] assemblies
        )
        {
            containerBuilder.RegisterModule(new GenericModule(assemblies));
            return containerBuilder;
        }

        /// <summary>
        /// Register controllers with Autofac
        /// </summary>
        /// <param name="containerBuilder">The container builder</param>
        /// <param name="assemblies">Assemblies to scan for controllers</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder AddControllers(
            this ContainerBuilder containerBuilder,
            params Assembly[] assemblies
        )
        {
            containerBuilder.RegisterModule(new ControllerModule(assemblies));
            return containerBuilder;
        }
    }
}
