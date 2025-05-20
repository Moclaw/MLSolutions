using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace sample.Application
{
    public static partial class Register
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            return services;
        }
    }
}
