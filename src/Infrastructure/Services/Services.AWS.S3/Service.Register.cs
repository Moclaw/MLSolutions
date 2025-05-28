using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.AWS.S3.Configurations;
using Services.AWS.S3.Interfaces;
using Services.AWS.S3.Services;

namespace Services.AWS.S3;

public static partial class Register
{
    /// <summary>
    /// Register AWS S3 services with default configuration section "AWS:S3"
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddS3Services(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services.AddS3Services(configuration, S3Configuration.SectionName);
    }

    /// <summary>
    /// Register AWS S3 services with custom configuration section
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="configSectionPath">Configuration section path</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddS3Services(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionPath
    )
    {
        services.Configure<S3Configuration>(configuration.GetSection(configSectionPath));
        services.AddScoped<IS3Service, S3Service>();

        return services;
    }

    /// <summary>
    /// Register AWS S3 services with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddS3Services(
        this IServiceCollection services,
        Action<S3Configuration> configureOptions
    )
    {
        services.Configure(configureOptions);
        services.AddScoped<IS3Service, S3Service>();

        return services;
    }
}
