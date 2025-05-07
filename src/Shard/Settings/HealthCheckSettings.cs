namespace Shard.Settings
{
    public class HealthCheckSettings
    {
        public string? Url { get; set; }
        public int Interval { get; set; } 
        public int Timeout { get; set; } 
        public int FailureThreshold { get; set; }
        public int SuccessThreshold { get; set; }
        public bool EnableDatabaseCheck { get; set; } 
    }
}