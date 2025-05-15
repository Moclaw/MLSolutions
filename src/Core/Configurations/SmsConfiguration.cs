namespace Core.Configurations
{
    public class SmsConfiguration
    {
        public const string SectionName = "SmsOptions";
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }
}
