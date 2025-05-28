using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.AWS.S3;

namespace sample.Application
{
    public static partial class Register
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
#pragma warning disable IDE0060 // Remove unused parameter
            IConfiguration configuration
        )
        {
            return services;
        }
    }
}
