using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.AWS.SecretsManager.Configurations;
using Services.AWS.SecretsManager.Interfaces;
using Services.AWS.SecretsManager.Services;

namespace Services.AWS.SecretsManager;

/// <summary>
/// Service registration extensions for AWS Secrets Manager
/// </summary>
public static partial class Register
{
    /// <summary>
    /// Register AWS Secrets Manager services with default configuration section "AWS:SecretsManager"
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSecretsManagerServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services.AddSecretsManagerServices(configuration, SecretsManagerConfiguration.SectionName);
    }

    /// <summary>
    /// Register AWS Secrets Manager services with custom configuration section
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="configSectionPath">Configuration section path</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSecretsManagerServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionPath
    )
    {
        services.Configure<SecretsManagerConfiguration>(configuration.GetSection(configSectionPath));
        services.AddScoped<ISecretsManagerService, SecretsManagerService>();
        
        // Add memory cache if not already registered (for caching secrets)
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Register AWS Secrets Manager services with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSecretsManagerServices(
        this IServiceCollection services,
        Action<SecretsManagerConfiguration> configureOptions
    )
    {
        services.Configure(configureOptions);
        services.AddScoped<ISecretsManagerService, SecretsManagerService>();
        
        // Add memory cache if not already registered (for caching secrets)
        services.AddMemoryCache();

        return services;
    }
}
