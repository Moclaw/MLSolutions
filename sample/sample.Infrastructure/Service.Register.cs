using DotnetCap;
using EfCore.IRepositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using sample.Domain.Constants;
using sample.Infrastructure.Repositories;

namespace sample.Infrastructure
{
    public static partial class Register
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDotnetCap(configuration).AddRabbitMq(configuration);

            services.AddKeyedScoped<ICommandRepository, CommandDefaultRepository>(
                ServiceKeys.CommandRepository
            );
            services.TryAddKeyedScoped(
                typeof(IQueryRepository<,>),
                ServiceKeys.QueryRepository,
                typeof(QueryDefaultRepository<,>)
            );

            return services;
        }
    }
}
