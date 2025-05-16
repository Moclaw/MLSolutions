using Core.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitOptionsConfig = Core.Configurations.RabbitMQOptions;

namespace DotnetCap.Rabbit
{
    public static class RabbitConfiguration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static void ConfigureRabbit(IConfiguration configuration, IServiceCollection services)
        {
            // Validate configuration
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            if (services == null)
                throw new ArgumentNullException(nameof(services), "Services collection cannot be null.");

            var capConfig = configuration.GetSection(DotnetCapConfiguration.SectionName).Get<RabbitOptionsConfig>() ?? throw new ArgumentNullException("RabbitMQ configuration is not set.");

            services.AddCap(options =>
            {
                options.UseRabbitMQ(rabbit =>
                {
                    rabbit.HostName = capConfig.HostName ?? "localhost";
                    rabbit.Port = capConfig.Port;
                    rabbit.UserName = capConfig.UserName ?? "guest";
                    rabbit.Password = capConfig.Password ?? "guest";
                    rabbit.VirtualHost = capConfig.VirtualHost ?? "/";
                    rabbit.ExchangeName = capConfig.ExchangeName ?? "cap.default.router";
                    rabbit.PublishConfirms = capConfig.PublishConfirms;

                    if (capConfig.QueueArguments is not null)
                    {
                        rabbit.QueueArguments = new DotNetCore.CAP.RabbitMQOptions.QueueArgumentsOptions
                        {
                            QueueMode = capConfig.QueueArguments.QueueMode ?? "default",
                            MessageTTL = capConfig.QueueArguments.MessageTTL,
                            QueueType = capConfig.QueueArguments.QueueType ?? "default"
                        };
                    }

                    if (capConfig.QueueOptions is not null)
                    {
                        rabbit.QueueOptions = new DotNetCore.CAP.RabbitMQOptions.QueueRabbitOptions
                        {
                            Durable = capConfig.QueueOptions.Durable,
                            Exclusive = capConfig.QueueOptions.Exclusive,
                            AutoDelete = capConfig.QueueOptions.AutoDelete
                        };
                    }

                    if (capConfig.BasicQosOptions is not null)
                    {
                        rabbit.BasicQosOptions = new DotNetCore.CAP.RabbitMQOptions.BasicQos(
                            capConfig.BasicQosOptions.PrefetchCount,
                            capConfig.BasicQosOptions.Global
                        );
                    }
                });
            });
        }
    }
}
