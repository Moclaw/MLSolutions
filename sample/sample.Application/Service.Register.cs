using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace sample.Application
{
    public static partial class Register
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
                .Where(t => t.Name.EndsWith("Service") && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var serviceType in serviceTypes)
            {
                var interfaces = serviceType
                    .GetInterfaces()
                    .Where(i => i.Name.EndsWith("Service"))
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
