namespace Core.Configurations
{
    /// <summary>
    /// Represents the configuration settings for DotnetCap integration.
    /// </summary>
    public class DotnetCapConfiguration
    {
        /// <summary>
        /// The configuration section name for DotnetCap.
        /// </summary>
        public const string SectionName = "DotnetCap";

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the database provider (e.g., SqlServer, MySql).
        /// </summary>
        public string? DbProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use transactions.
        /// </summary>
        public bool UseTransaction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the CAP dashboard.
        /// </summary>
        public bool UseDashboard { get; set; }

        /// <summary>
        /// Gets or sets the path for the CAP dashboard.
        /// </summary>
        public string? DashboardPath { get; set; }

        /// <summary>
        /// Gets or sets the username for the CAP dashboard.
        /// </summary>
        public string? DashboardUser { get; set; }

        /// <summary>
        /// Gets or sets the password for the CAP dashboard.
        /// </summary>
        public string? DashboardPassword { get; set; }

        /// <summary>
        /// Gets or sets the number of times to retry a failed message.
        /// </summary>
        public int FailedRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the interval (in seconds) between failed message retries.
        /// </summary>
        public int FailedRetryInterval { get; set; }

        /// <summary>
        /// Gets or sets the time (in seconds) after which succeeded messages expire.
        /// </summary>
        public int SucceedMessageExpiredAfter { get; set; }

        /// <summary>
        /// Gets or sets the Kafka configuration options.
        /// </summary>
        public KafkaOptions? Kafka { get; set; }

        /// <summary>
        /// Gets or sets the RabbitMQ configuration options.
        /// </summary>
        public RabbitMQOptions? RabbitMQ { get; set; }
    }

}
