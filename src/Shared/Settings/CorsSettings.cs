namespace Shared.Settings
{
    /// <summary>
    /// Represents the settings for configuring Cross-Origin Resource Sharing (CORS).
    /// </summary>
    public class CorsSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether localhost is allowed for CORS requests.
        /// </summary>
        public bool IsAllowLocalhost { get; set; } = true;

        /// <summary>
        /// Gets or sets the default CORS policy name.
        /// </summary>
        public string? DefaultPolicy { get; set; }

        /// <summary>
        /// Gets or sets the list of custom CORS policies.
        /// </summary>
        public List<CorsPolicy>? Policies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether preflight requests are enabled.
        /// </summary>
        public bool EnablePreflightRequests { get; set; } = true;
    }

    /// <summary>
    /// Represents a single CORS policy configuration.
    /// </summary>
    public class CorsPolicy
    {
        /// <summary>
        /// Gets or sets the name of the CORS policy.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether any origin is allowed.
        /// </summary>
        public bool AllowAnyOrigin { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed origins for the CORS policy.
        /// </summary>
        public List<string>? AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any HTTP method is allowed.
        /// </summary>
        public bool AllowAnyMethod { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed HTTP methods for the CORS policy.
        /// </summary>
        public List<string>? AllowedMethods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any HTTP header is allowed.
        /// </summary>
        public bool AllowAnyHeader { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed HTTP headers for the CORS policy.
        /// </summary>
        public List<string>? AllowedHeaders { get; set; }


    }
}
