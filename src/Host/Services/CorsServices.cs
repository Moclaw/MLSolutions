using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shard.Constants;

namespace Host.Services;

public static class CorsServices
{
    public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var corsSettings = configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>();
        
        if (corsSettings == null || corsSettings.Policies == null || corsSettings.Policies.Count == 0)
        {
            throw new InvalidOperationException("Invalid or missing CorsSettings configuration.");
        }

        services.AddCors(options =>
        {
            if (corsSettings.IsAllowLocalhost)
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            }

            foreach (var policy in corsSettings.Policies)
            {
                options.AddPolicy(policy.Name, builder =>
                {
                    if (policy.AllowAnyOrigin)
                    {
                        builder.AllowAnyOrigin();
                    }
                    else if (policy.AllowedOrigins != null)
                    {
                        builder.WithOrigins([.. policy.AllowedOrigins]);
                    }

                    if (policy.AllowAnyMethod)
                    {
                        builder.AllowAnyMethod();
                    }
                    else if (policy.AllowedMethods != null)
                    {
                        builder.WithMethods([.. policy.AllowedMethods]);
                    }

                    if (policy.AllowAnyHeader)
                    {
                        builder.AllowAnyHeader();
                    }
                    else if (policy.AllowedHeaders != null)
                    {
                        builder.WithHeaders([.. policy.AllowedHeaders]);
                    }
                });
            }
        });

        return services;
    }

    public static IApplicationBuilder UseCorsServices(this IApplicationBuilder app, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>();
        if (corsSettings == null || corsSettings.DefaultPolicy == null)
        {
            throw new InvalidOperationException("Invalid or missing CorsSettings configuration.");
        }

        app.UseCors(corsSettings.DefaultPolicy);

        if (corsSettings.EnablePreflightRequests)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 204; 
                    return;
                }
                await next();
            });
        }

        return app;
    }
}
