using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Services.Autofac.Modules;

namespace sample.Application.DependencyInjection
{
    public static class AutofacExtensions
    {
        /// <summary>
        /// Adds Autofac as the service provider factory and registers application services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The Autofac service provider factory</returns>
        public static AutofacServiceProviderFactory AddAutofacWithApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Register application services
            services.AddApplicationServices(configuration);

            // Create and return the Autofac service provider factory
            var factory = new AutofacServiceProviderFactory(builder =>
            {
                // Get the application assembly
                var applicationAssembly = Assembly.GetExecutingAssembly();

                // Get the domain assembly which is referenced by the application assembly
                var domainAssembly = Assembly.Load("sample.Domain");

                // Get the infrastructure assembly which is referenced by the application assembly
                var infrastructureAssembly = Assembly.Load("sample.Infrastructure");                // Get all relevant assemblies
                var assemblies = new[]
                {
                    applicationAssembly,
                    domainAssembly,
                    infrastructureAssembly
                };

                // Register by convention
                RegisterServicesByConvention(builder, assemblies);

                // Register repositories
                builder.RegisterModule(new RepositoryModule(assemblies: assemblies));

                // Register services
                builder.RegisterModule(new ServiceModule(assemblies: assemblies));

                // Register attribute-based services
                builder.RegisterModule(new AttributeModule(assemblies));

                // Allow property injection for controllers
                builder.RegisterSource(
                    new Autofac.Features.ResolveAnything.AnyConcreteTypeNotAlreadyRegisteredSource()
                );
            });            return factory;
        }

        /// <summary>
        /// Register services by convention
        /// </summary>
        private static void RegisterServicesByConvention(ContainerBuilder builder, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                // Register classes that end with "Service"
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Service") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                // Register classes that end with "Repository" 
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Repository") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

                // Register other common patterns
                builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Manager") && !t.IsAbstract)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
