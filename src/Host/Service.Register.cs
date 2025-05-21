using Elastic.Apm.NetCoreAll;
using HealthChecks.UI.Client;
using Host.Filters;
using Host.Handlers;
using Host.Middleware;
using Host.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Shared.Settings;

namespace Host;

public static partial class Register
{
    public static IServiceCollection AddGlobalExceptionHandling(
        this IServiceCollection services,
        string applicationName
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        services.AddSingleton<IExceptionHandlerFactory, ExceptionHandlerFactory>();

        return services;
    }

    public static IApplicationBuilder UseGlobalExceptionHandling(
        this IApplicationBuilder builder
    )
    {
        return builder
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseStatusCodePagesWithReExecute("/Error/{0}");
    }

    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder,
        IConfiguration configuration, string applicationName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        builder.Host.UseSerilog((context, services, serilogOptions) =>
        {
            serilogOptions
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.FromLogContext();
        });

        builder.Services.AddSingleton<IStartupFilter>(new StartupFilter(applicationName));

        return builder;
    }



    public static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger>();

        var healthCheckSettings = configuration.GetSection(nameof(HealthCheckSettings)).Get<HealthCheckSettings>();

        if (healthCheckSettings == null)
        {
            logger.Warning("HealthCheckSettings configuration is missing.");
            return services;
        }

        if (healthCheckSettings.EnableDatabaseCheck)
        {
            logger.Information("Database health checks are enabled.");
            var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DbContext)) && !t.IsAbstract)
                .ToList();

            foreach (var dbContextType in dbContextTypes)
            {
                Log.Information("Adding health check for DbContext: {DbContextType}", dbContextType.Name);
                services.AddHealthChecks()
                    .AddDbContextCheck(dbContextType, name: dbContextType.Name, failureStatus: HealthStatus.Degraded);
            }
        }

        var elasticHealthCheckSettings = configuration.GetSection(nameof(ElasticSearchSettings)).Get<ElasticSearchSettings>();
        if (elasticHealthCheckSettings != null && !string.IsNullOrWhiteSpace(elasticHealthCheckSettings.Url))
        {
            logger.Information("Adding health check for ElasticSearch at URL: {ElasticUrl}", elasticHealthCheckSettings.Url);
            services.AddHealthChecks()
                .AddUrlGroup(new Uri(elasticHealthCheckSettings.Url), name: elasticHealthCheckSettings.Name, failureStatus: HealthStatus.Degraded);
        }

        logger.Information("Adding self health check.");
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["ready"]);

        logger.Information("Health check configuration completed.");
        return services;
    }

    public static IHealthChecksBuilder AddDbContextCheck(
        this IHealthChecksBuilder builder,
        Type dbContextType,
        string name,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        if (!typeof(DbContext).IsAssignableFrom(dbContextType))
        {
            throw new ArgumentException($"The type {dbContextType.Name} must inherit from DbContext.", nameof(dbContextType));
        }

        builder.Add(new HealthCheckRegistration(
            name,
            sp =>
            {
                if (sp.GetRequiredService(dbContextType) is not DbContext dbContext)
                {
                    throw new InvalidOperationException($"Unable to resolve DbContext of type {dbContextType.Name}.");
                }

                return new DbContextHealthCheck(dbContext);
            },
            failureStatus,
            tags,
            timeout));

        return builder;
    }


    public static IApplicationBuilder UseElasticApm(
       this IApplicationBuilder builder,
       IConfiguration configuration)
    {
        builder.UseAllElasticApm(configuration);
        return builder;
    }

    public static IApplicationBuilder UseHealthChecks(
        this IApplicationBuilder builder,
        IConfiguration configuration)
    {
        var healthCheckSettings = configuration.GetSection(nameof(HealthCheckSettings)).Get<HealthCheckSettings>() ?? throw new InvalidOperationException("HealthCheckSettings configuration is missing.");

        builder.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks(healthCheckSettings?.Path ?? "health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        });
        return builder;
    }
}