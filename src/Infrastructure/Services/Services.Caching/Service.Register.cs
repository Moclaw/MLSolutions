using Core.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Services.Caching
{
    public static partial class Register
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static IServiceCollection AddRedisCache(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var redisConfiguration = configuration.GetSection(RedisConfiguration.SectionName).Get<RedisConfiguration>();
            if (redisConfiguration is null)
                throw new ArgumentNullException(nameof(redisConfiguration), "Redis configuration is not set.");

            if (string.IsNullOrWhiteSpace(redisConfiguration.Connection))
                throw new ArgumentException(
                    "Redis connection string is not set.", nameof(redisConfiguration.Connection));

            if (string.IsNullOrWhiteSpace(redisConfiguration.InstanceName))
                throw new ArgumentException("Redis instance name is not set.", nameof(redisConfiguration.InstanceName));

            if (redisConfiguration.Database < 0)
                throw new ArgumentOutOfRangeException(nameof(redisConfiguration.Database), "Redis database number must be non-negative.");

            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var configOptions = ConfigurationOptions.Parse(redisConfiguration.Connection, true);
                configOptions.ClientName = redisConfiguration.InstanceName;
                configOptions.DefaultDatabase = redisConfiguration.Database;
                return ConnectionMultiplexer.Connect(configOptions);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConfiguration.Connection;
                options.InstanceName = redisConfiguration.InstanceName;
            });

            return services;
        }
    }
}
