using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace sample.Application
{
    public static partial class Register
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
#pragma warning disable IDE0060 // Remove unused parameter
            IConfiguration configuration
#pragma warning restore IDE0060 // Remove unused parameter
        ) => services;
    }
}
