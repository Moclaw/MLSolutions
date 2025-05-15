using Microsoft.Extensions.DependencyInjection;

namespace EfCore
{
    public static partial class Register
    {
        public static IServiceCollection AddGlobalExceptionHandling(
            this IServiceCollection services
        )
        {
            return services;
        }

    }
}
