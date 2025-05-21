using EfCore.IRepositories;
using EfCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using sample.Domain.Constants;
using sample.Infrastructure.Persistence.EfCore;
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
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
            // Register repositories
            services.AddKeyedScoped(
                typeof(ICommandRepository),
                ServiceKeys.CommandRepository,
                typeof(CommandDefaultRepository)
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
