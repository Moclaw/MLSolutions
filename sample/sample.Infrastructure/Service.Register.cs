using Autofac;
using EfCore.IRepositories;
using EfCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using sample.Domain.Constants;
using sample.Infrastructure.Persistence.EfCore;
using sample.Infrastructure.Repositories;
using Services.Autofac.Extensions;

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
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"))
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, LogLevel.Information)
                );
            // Register repositories
            services.AddKeyedScoped<ICommandRepository, CommandDefaultRepository>(ServiceKeys.CommandRepository);
            
            services.TryAddKeyedScoped(
                typeof(IQueryRepository<,>),
                ServiceKeys.QueryRepository,
                typeof(QueryDefaultRepository<,>)
            );

            //S3 storage, caching, and other services can be registered here

            return services;
        }        /// <summary>
        /// Register Infrastructure services with Autofac
        /// </summary>
        /// <param name="builder">Autofac ContainerBuilder</param>
        public static ContainerBuilder AddInfrastructureServices(this ContainerBuilder builder)
        {
            // Register services from this assembly using attribute-based registration
            builder.RegisterServiceAssemblies(true, false, typeof(Register).Assembly);
            
            return builder;
        }
    }
}
