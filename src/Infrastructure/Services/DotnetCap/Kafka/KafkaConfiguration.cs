using Core.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KafkaOptionsConfig = Core.Configurations.KafkaOptions;

namespace DotnetCap.Kafka
{
    internal static class KafkaConfiguration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static void ConfigureKafka(IConfiguration configuration, IServiceCollection services)
        {
            // Validate configuration
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            if (services == null)
                throw new ArgumentNullException(nameof(services), "Services collection cannot be null.");

            // Retrieve Kafka settings from configuration
            var kafkaSettingsSection = configuration.GetSection(DotnetCapConfiguration.SectionName).GetSection(KafkaOptionsConfig.SectionName);
            if (kafkaSettingsSection == null || !kafkaSettingsSection.GetChildren().Any())
                throw new ArgumentException("Kafka settings section is not found in the configuration.", nameof(configuration));

            var capConfig = kafkaSettingsSection.Get<KafkaOptionsConfig>() ??
                throw new ArgumentNullException("Kafka settings cannot be null.");

            services.AddCap(options =>
            {
                options.UseKafka(kafka =>
                {
                    kafka.Servers = string.Join(",", capConfig.BootstrapServers ?? []);
                    kafka.ConnectionPoolSize = capConfig.ConnectionPoolSize;

                    foreach (var kv in capConfig.MainConfig ?? [])
                    {
                        kafka.MainConfig[kv.Key] = kv.Value;
                    }

                    if (!string.IsNullOrEmpty(capConfig.GroupId))
                        kafka.MainConfig["group.id"] = capConfig.GroupId;

                    if (!string.IsNullOrEmpty(capConfig.ClientId))
                        kafka.MainConfig["client.id"] = capConfig.ClientId;

                    if (!string.IsNullOrEmpty(capConfig.SecurityProtocol))
                        kafka.MainConfig["security.protocol"] = capConfig.SecurityProtocol;

                    if (!string.IsNullOrEmpty(capConfig.SaslMechanism))
                        kafka.MainConfig["sasl.mechanism"] = capConfig.SaslMechanism;

                    if (!string.IsNullOrEmpty(capConfig.SaslUsername))
                        kafka.MainConfig["sasl.username"] = capConfig.SaslUsername;

                    if (!string.IsNullOrEmpty(capConfig.SaslPassword))
                        kafka.MainConfig["sasl.password"] = capConfig.SaslPassword;

                    if (capConfig.TopicOptions != null)
                    {
                        kafka.TopicOptions = new DotNetCore.CAP.KafkaTopicOptions
                        {
                            NumPartitions = capConfig.TopicOptions.NumPartitions,
                            ReplicationFactor = capConfig.TopicOptions.ReplicationFactor
                        };
                    }

                    if (capConfig.RetriableErrorCodes?.Count > 0)
                    {
                        kafka.RetriableErrorCodes = [.. capConfig.RetriableErrorCodes.Select(i => (Confluent.Kafka.ErrorCode)i)];
                    }

                });
            });
        }
    }
}
