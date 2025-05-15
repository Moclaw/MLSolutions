using DotnetCap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace sample.Infrastructure
{
    public static partial class Register
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
