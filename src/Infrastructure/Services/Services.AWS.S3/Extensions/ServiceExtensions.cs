using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.AWS.S3.Configurations;
using Services.AWS.S3.Interfaces;
using Services.AWS.S3.Services;

namespace Services.AWS.S3.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddS3Service(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionPath = "AWS:S3"
    )
    {
        services.Configure<S3Configuration>(configuration.GetSection(configSectionPath));
        services.AddScoped<IS3Service, S3Service>();

        return services;
    }
}
