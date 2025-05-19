namespace Shard.Settings
{
    public record HealthCheckSettings
    {
        public string? Path { get; init; }
        public int Interval { get; init; }
        public int Timeout { get; init; }
        public int FailureThreshold { get; init; }
        public int SuccessThreshold { get; init; }
        public bool EnableDatabaseCheck { get; init; }
    }
}