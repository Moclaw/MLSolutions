using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.AWS.S3;
using Services.AWS.SecretsManager;
using Services.Autofac.Extensions;

namespace sample.Application
{
    public static partial class Register
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
#pragma warning disable IDE0060 // Remove unused parameter
            IConfiguration configuration
        )
        {
            // Register AWS S3 services with default configuration section "AWS:S3"
            services.AddS3Services(configuration);
            
            // Register AWS Secrets Manager services with default configuration section "AWS:SecretsManager"
            services.AddSecretsManagerServices(configuration);
            
            return services;
        }        /// <summary>
        /// Register Application services with Autofac
        /// </summary>
        /// <param name="builder">Autofac ContainerBuilder</param>
        public static ContainerBuilder AddApplicationServices(this ContainerBuilder builder)
        {
            // Register services from this assembly using attribute-based registration
            builder.RegisterServiceAssemblies(true, false, typeof(Register).Assembly);
            
            return builder;
        }
    }
}
