using Shard.Settings;
using System.Text;
using System.Text.Json;

namespace MLSolutions;
/// <summary>
/// Provides extension methods for object manipulation, including string and byte array conversions,
/// and JSON serialization.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Converts a byte array to a string using the specified encoding.
    /// </summary>
    /// <param name="orignal">The byte array to convert.</param>
    /// <param name="encoding">The encoding to use for conversion. Defaults to <see cref="Encoding.UTF8"/> if not specified.</param>
    /// <returns>A string representation of the byte array, or an empty string if the array is null or empty.</returns>
    public static string ConvertToString(this byte[]? orignal, Encoding? encoding = null)
    {
        if (orignal is not { Length: > 0 }) return string.Empty;

        encoding ??= Encoding.UTF8;

        return encoding.GetString(orignal);
    }

    /// <summary>
    /// Converts a string to a byte array using the specified encoding.
    /// </summary>
    /// <param name="orignal">The string to convert.</param>
    /// <param name="encoding">The encoding to use for conversion. Defaults to <see cref="Encoding.UTF8"/> if not specified.</param>
    /// <returns>A byte array representation of the string, or an empty byte array if the string is null or empty.</returns>
    public static byte[] ToByteArray(this string orignal, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(orignal)) return Array.Empty<byte>();

        encoding ??= Encoding.UTF8;

        return encoding.GetBytes(orignal);
    }

    /// <summary>
    /// Serializes an object to a JSON string using the specified <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="data">The object to serialize.</param>
    /// <param name="option">The JSON serializer options to use. Defaults to <see cref="JsonSettings.DefaultOptions"/> if not specified.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string Serialize<T>(this T data, JsonSerializerOptions? option = null)
    {
        option ??= JsonSettings.DefaultOptions;

        return JsonSerializer.Serialize(data, option);
    }
}