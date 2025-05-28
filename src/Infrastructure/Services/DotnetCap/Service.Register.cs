using Core.Configurations;
using Core.Enums;
using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static DotnetCap.Kafka.KafkaConfiguration;
using static DotnetCap.Rabbit.RabbitConfiguration;

namespace DotnetCap
{
    public static class Register
    {
        public static IServiceCollection AddDotnetCap(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var dotnetCapConfiguration =
                configuration.GetSection(DotnetCapConfiguration.SectionName).Get<DotnetCapConfiguration>()
                ?? null;

            if (dotnetCapConfiguration == null)
                return services;

            ValidateDotnetCapConfiguration(dotnetCapConfiguration);

            if (dotnetCapConfiguration.DbProvider == null) return services;

            var dbProvider = ParseDbProvider(dotnetCapConfiguration.DbProvider);

            services.AddCap(x =>
            {
                if (dotnetCapConfiguration.ConnectionString != null)
                    ConfigureDatabase(x, dbProvider, dotnetCapConfiguration.ConnectionString);

                if (dotnetCapConfiguration.UseDashboard)
                {
                    ConfigureDashboard(x, dotnetCapConfiguration);
                }
            });

            return services;
        }

        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var dotnetCapConfiguration =
                configuration.GetSection(DotnetCapConfiguration.SectionName).Get<DotnetCapConfiguration>()
                ?? null;
            if (dotnetCapConfiguration == null)
                return services;

            ConfigureRabbit(configuration, services);

            return services;
        }

        public static IServiceCollection AddKafka(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var dotnetCapConfiguration =
                configuration.GetSection(DotnetCapConfiguration.SectionName).Get<DotnetCapConfiguration>()
                ?? null;
            if (dotnetCapConfiguration == null)
                return services;

            ConfigureKafka(configuration, services);

            return services;
        }

        private static void ValidateDotnetCapConfiguration(DotnetCapConfiguration config)
        {
#pragma warning disable CA2208
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
                throw new ArgumentException("DotnetCap connection string is not set.", nameof(config.ConnectionString));
            if (string.IsNullOrWhiteSpace(config.DbProvider))
                throw new ArgumentException("DotnetCap database provider is not set.", nameof(config.DbProvider));
            if (config.FailedRetryCount < 0)
                throw new ArgumentOutOfRangeException(nameof(config.FailedRetryCount),
                    "DotnetCap failed retry count must be non-negative.");
            if (config.FailedRetryInterval < 0)
                throw new ArgumentOutOfRangeException(nameof(config.FailedRetryInterval),
                    "DotnetCap failed retry interval must be non-negative.");
            if (config.SucceedMessageExpiredAfter < 0)
                throw new ArgumentOutOfRangeException(nameof(config.SucceedMessageExpiredAfter),
                    "DotnetCap succeeded expire time must be non-negative.");

            if (!config.UseDashboard) return;
            if (string.IsNullOrWhiteSpace(config.DashboardPath))
                throw new ArgumentException("DotnetCap dashboard path is not set.", nameof(config.DashboardPath));
            if (string.IsNullOrWhiteSpace(config.DashboardUser))
                throw new ArgumentException("DotnetCap dashboard user is not set.", nameof(config.DashboardUser));
            if (string.IsNullOrWhiteSpace(config.DashboardPassword))
                throw new ArgumentException("DotnetCap dashboard password is not set.",
                    nameof(config.DashboardPassword));
#pragma warning restore CA2208
        }

        private static DbProvider ParseDbProvider(string dbProvider) => Enum.TryParse<DbProvider>(dbProvider, true, out var provider)
                ? provider
                : throw new ArgumentException($"Invalid database provider: {dbProvider}", nameof(dbProvider));

        private static void ConfigureDatabase(CapOptions capOptions, DbProvider dbProvider, string connectionString)
        {
            switch (dbProvider)
            {
                case DbProvider.SqlServer:
                    capOptions.UseSqlServer(connectionString);
                    break;
                case DbProvider.MySql:
                    capOptions.UseMySql(connectionString);
                    break;
                case DbProvider.PostgreSql:
                    capOptions.UsePostgreSql(connectionString);
                    break;
                case DbProvider.MongoDB:
                    capOptions.UseMongoDB(connectionString);
                    break;
                case DbProvider.Oracle:
                case DbProvider.Sqlite:
                case DbProvider.InMemory:
                default:
                    throw new NotSupportedException($"Database provider '{dbProvider}' is not supported.");
            }
        }

        private static void ConfigureDashboard(CapOptions capOptions, DotnetCapConfiguration config) => capOptions.UseDashboard(dashboard =>
                                                                                                                 {
                                                                                                                     dashboard.PathMatch = config.DashboardPath ?? "/cap";
                                                                                                                 });
    }
}