namespace Services.AWS.S3.Configurations;

public class S3Configuration
{
    public const string SectionName = "AWS:S3";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool UseCredentialsFile { get; set; } = false;
    public string CredentialsFilePath { get; set; } = string.Empty;
    
    // LocalStack support
    public bool UseLocalStack { get; set; } = false;
    public string LocalStackServiceUrl { get; set; } = "http://localhost:4566";
    public bool ForcePathStyle { get; set; } = true;
}
