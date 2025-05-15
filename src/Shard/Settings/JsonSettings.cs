using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shard.Settings;

/// <summary>
/// Provides default JSON serialization settings for the application.
/// </summary>
public readonly struct JsonSettings
{
    /// <summary>
    /// The default <see cref="JsonSerializerOptions"/> used for JSON serialization and deserialization.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        /// <summary>
        /// Specifies the maximum depth allowed when reading JSON. Default is 256.
        /// </summary>
        MaxDepth = 256,

        /// <summary>
        /// Indicates whether the JSON output should be formatted with indentation for readability.
        /// </summary>
        WriteIndented = true,

        /// <summary>
        /// Configures the handling of object references to ignore cycles during serialization.
        /// </summary>
        ReferenceHandler = ReferenceHandler.IgnoreCycles,

        /// <summary>
        /// Specifies the condition to ignore properties during serialization. Default is to never ignore properties.
        /// </summary>
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,

        /// <summary>
        /// Indicates whether fields should be included in serialization and deserialization.
        /// </summary>
        IncludeFields = true,

        /// <summary>
        /// Specifies the naming policy for property names. Default is camelCase.
        /// </summary>
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
