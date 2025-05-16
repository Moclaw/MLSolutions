using DotnetCap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace sample.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration

        )
        {
            services.AddDotnetCap(configuration)
                    .AddRabbitMq(configuration);

            return services;
        }
    }
}
