namespace Services.AWS.SecretsManager.Configurations;

/// <summary>
/// Configuration class for AWS Secrets Manager service
/// </summary>
public class SecretsManagerConfiguration
{
    /// <summary>
    /// Default configuration section name
    /// </summary>
    public const string SectionName = "AWS:SecretsManager";
    
    /// <summary>
    /// AWS Access Key ID
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;
    
    /// <summary>
    /// AWS Secret Access Key
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// AWS Region
    /// </summary>
    public string Region { get; set; } = "us-east-1";
    
    /// <summary>
    /// Whether to use AWS credentials file instead of access keys
    /// </summary>
    public bool UseCredentialsFile { get; set; } = false;
    
    /// <summary>
    /// Path to AWS credentials file
    /// </summary>
    public string CredentialsFilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// AWS Profile name to use from credentials file
    /// </summary>
    public string ProfileName { get; set; } = "default";
    
    /// <summary>
    /// LocalStack support for local development
    /// </summary>
    public bool UseLocalStack { get; set; } = false;
    
    /// <summary>
    /// LocalStack service URL
    /// </summary>
    public string LocalStackServiceUrl { get; set; } = "http://localhost:4566";
    
    /// <summary>
    /// Default prefix for secret names (optional)
    /// </summary>
    public string SecretPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to cache secrets in memory for performance
    /// </summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>
    /// Cache expiration time in minutes
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 15;
}
