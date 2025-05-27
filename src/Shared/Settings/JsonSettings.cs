using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Settings;

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
        MaxDepth = 256,

        WriteIndented = true,
     
        ReferenceHandler = ReferenceHandler.IgnoreCycles,

        DefaultIgnoreCondition = JsonIgnoreCondition.Never,

        IncludeFields = true,

        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
