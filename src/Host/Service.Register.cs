using Core.Configuration;
using Core.Constants;
using Host.Middleware;
using Host.Services;
using Microsoft.AspNetCore.Builder;
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

        ModuleConfiguration config = new()
        {
            Cache = new MemoryCacheDefintion(),
            Assemblies = [typeof(Register).Assembly],
            DisableMediaR = true,
        };

        services.AddSingleton(config);

        services.AddSingleton<IExceptionHandlerFactory, ExceptionHandlerFactory>();

        return services;
    }

    public static IApplicationBuilder UseFrameworkExceptionHandling(
        this IApplicationBuilder builder
    )
    {
        return builder
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseStatusCodePagesWithReExecute("/Error/{0}");
    }

    public static WebApplicationBuilder AddApplicationLog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(
            (context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.WithProperty(
                        "ApplicationName",
                        context.HostingEnvironment.ApplicationName
                    )
                    .Enrich.FromLogContext();
            }
        );
        return builder;
    }
}
