using System.Reflection;
using Core.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace sample.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Register all application services
            var assembly = Assembly.GetExecutingAssembly();

            // Register services by naming convention
            var serviceTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.Name.EndsWith(AutofacConstants.ServiceConventions.Service)
                    && t is { IsInterface: false, IsAbstract: false }
                )
                .ToList();

            foreach (var serviceType in serviceTypes)
            {
                var interfaces = serviceType
                    .GetInterfaces()
                    .Where(i => i.Name.EndsWith(AutofacConstants.ServiceConventions.Service))
                    .ToList();

                if (interfaces.Any())
                {
                    foreach (var serviceInterface in interfaces)
                    {
                        services.AddScoped(serviceInterface, serviceType);
                    }
                }
                else
                {
                    services.AddScoped(serviceType);
                }
            }

            return services;
        }
    }
}
