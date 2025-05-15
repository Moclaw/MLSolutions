namespace Core.Configurations
{
    public class RedisConfiguration
    {
        public const string SectionName = "Redis";

        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        public string Connection { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Redis instance name.
        /// </summary>
        public string InstanceName { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Redis database number.
        /// </summary>
        public int Database { get; set; } = 0;
    }
}
