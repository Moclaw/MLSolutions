using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Autofac.Modules;
using System.Reflection;

namespace Services.Autofac.Extensions
{
    public static class AutofacExtensions
    {
        /// <summary>
        /// Register services using Autofac with enhanced assembly scanning and convention-based registration
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureContainer">Optional delegate to configure the container</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The host builder</returns>
        public static IHostBuilder UseAutofacServiceProvider(
            this IHostBuilder hostBuilder,
            Action<ContainerBuilder>? configureContainer = null,
            params Assembly[] assemblies
        ) => hostBuilder
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    // Register services from specified assemblies
                    if (assemblies.Length > 0)
                    {
                        builder.RegisterModule(new ServiceModule(assemblies));
                    }

                    // Apply additional container configuration if provided
                    configureContainer?.Invoke(builder);
                });

        /// <summary>
        /// Configure Autofac container for ASP.NET Core applications
        /// </summary>
        /// <param name="builder">The WebApplicationBuilder</param>
        /// <param name="configureContainer">Optional delegate to configure the container</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The WebApplicationBuilder</returns>
        public static WebApplicationBuilder UseAutofacServiceProvider(
            this WebApplicationBuilder builder,
            Action<ContainerBuilder>? configureContainer = null,
            params Assembly[] assemblies
        )
        {
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                // Register services from specified assemblies
                if (assemblies.Length > 0)
                {
                    containerBuilder.RegisterModule(new ServiceModule(assemblies));
                }

                // Apply additional container configuration if provided
                configureContainer?.Invoke(containerBuilder);
            });

            return builder;
        }

        /// <summary>
        /// Register services using Autofac
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureContainer">Optional delegate to configure the container</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The service collection wrapped with Autofac</returns>
        public static AutofacServiceProvider UseAutofacServiceProvider(
            this IServiceCollection services,
            Action<ContainerBuilder>? configureContainer = null,
            params Assembly[] assemblies
        )
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);

            // Register services from specified assemblies
            if (assemblies.Length > 0)
            {
                containerBuilder.RegisterModule(new ServiceModule(assemblies));
            }

            // Apply additional container configuration if provided
            configureContainer?.Invoke(containerBuilder);

            var container = containerBuilder.Build();
            return new AutofacServiceProvider(container);
        }

        /// <summary>
        /// Register assemblies that contain services
        /// </summary>
        /// <param name="containerBuilder">The container builder</param>
        /// <param name="registerByNamingConvention">Whether to register services by naming convention</param>
        /// <param name="autoRegisterConcreteTypes">Whether to auto-register concrete types</param>
        /// <param name="assemblies">Assemblies to scan for services</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder RegisterServiceAssemblies(
            this ContainerBuilder containerBuilder,
            bool registerByNamingConvention = true,
            bool autoRegisterConcreteTypes = false,
            params Assembly[] assemblies
        )
        {
            containerBuilder.RegisterModule(
                new ServiceModule(assemblies, registerByNamingConvention, autoRegisterConcreteTypes)
            );
            return containerBuilder;
        }

        /// <summary>
        /// Register services by type
        /// </summary>
        /// <param name="containerBuilder">The container builder</param>
        /// <param name="serviceType">The service type</param>
        /// <param name="implementationType">The implementation type</param>
        /// <param name="lifetime">The lifetime scope</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder RegisterService(
            this ContainerBuilder containerBuilder,
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime = ServiceLifetime.Scoped
        )
        {
            var registration = containerBuilder.RegisterType(implementationType).As(serviceType);

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

            return containerBuilder;
        }

        /// <summary>
        /// Register services by type
        /// </summary>
        /// <typeparam name="TService">The service type</typeparam>
        /// <typeparam name="TImplementation">The implementation type</typeparam>
        /// <param name="containerBuilder">The container builder</param>
        /// <param name="lifetime">The lifetime scope</param>
        /// <returns>The container builder</returns>
        public static ContainerBuilder RegisterService<TService, TImplementation>(
            this ContainerBuilder containerBuilder,
            ServiceLifetime lifetime = ServiceLifetime.Scoped
        )
            where TService : class
            where TImplementation : class, TService => containerBuilder.RegisterService(
                typeof(TService),
                typeof(TImplementation),
                lifetime
            );
    }
}
