using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using sample.Infrastructure;

namespace sample.Application
{
    public static partial class Register
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration

        )
        {
            services.AddInfrastructureServices(configuration);

            return services;
        }
    }
}
