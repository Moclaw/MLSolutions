using Host.Middleware;
using Host.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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

    public static WebApplicationBuilder AddApplicationLog(this WebApplicationBuilder builder,
        IConfiguration configuration, string applicationName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console();
        });

        var logtest = Log.Logger
            .ForContext("MachineName", Environment.MachineName)
            .ForContext("ApplicationName", applicationName)
            .ForContext("Environment", builder.Environment.EnvironmentName);

        return builder;
    }
}
