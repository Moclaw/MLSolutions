using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shard.Settings;

public struct JsonSettings
{
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
