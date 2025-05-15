#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Core.Configurations;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents the configuration options for Kafka.
/// </summary>
public class KafkaOptions
{
    public const string SectionName = "Kafka";

    /// <summary>
    /// Gets or sets the list of Kafka bootstrap servers.
    /// </summary>
    public string[]? BootstrapServers { get; set; }

    /// <summary>
    /// Gets or sets the Kafka consumer group ID.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets the Kafka client ID.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the Kafka topic name.
    /// </summary>
    public string? Topic { get; set; }

    /// <summary>
    /// Gets or sets the security protocol for Kafka.
    /// </summary>
    public string? SecurityProtocol { get; set; }

    /// <summary>
    /// Gets or sets the SASL mechanism for Kafka authentication.
    /// </summary>
    public string? SaslMechanism { get; set; }

    /// <summary>
    /// Gets or sets the SASL username for Kafka authentication.
    /// </summary>
    public string? SaslUsername { get; set; }

    /// <summary>
    /// Gets or sets the SASL password for Kafka authentication.
    /// </summary>
    public string? SaslPassword { get; set; }

    /// <summary>
    /// Gets or sets the Kafka connection pool size (CAP default is 10).
    /// </summary>
    public int ConnectionPoolSize { get; set; } = 10;

    /// <summary>
    /// Optional main config overrides (librdkafka config).
    /// </summary>
    public Dictionary<string, string>? MainConfig { get; set; } = [];

    /// <summary>
    /// Optional topic-level configuration.
    /// </summary>
    public KafkaTopicOptions? TopicOptions { get; set; } = new();

    /// <summary>
    /// List of Kafka retriable error codes (numeric).
    /// </summary>
    public List<int>? RetriableErrorCodes { get; set; } = [];
}

public class KafkaTopicOptions
{
    /// <summary>
    /// The number of partitions for the new topic.
    /// </summary>
    public short NumPartitions { get; set; } = -1;

    /// <summary>
    /// The replication factor for the new topic.
    /// </summary>
    public short ReplicationFactor { get; set; } = -1;
}
