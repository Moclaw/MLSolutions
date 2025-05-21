using Shared.Settings;
using System.Text;
using System.Text.Json;

namespace MLSolutions;
/// <summary>
/// Provides extension methods for string manipulation and processing.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Normalizes a string by converting it to lowercase, removing non-alphanumeric characters,
    /// and replacing groups of whitespace with a single underscore.
    /// </summary>
    /// <param name="name">The input string to normalize.</param>
    /// <returns>A normalized string with only lowercase alphanumeric characters and underscores.</returns>
    public static string ToNormalize(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var sb = new StringBuilder(name.Length);
        bool pendingUnderscore = false;

        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (pendingUnderscore && sb.Length > 0)
                {
                    sb.Append('_');
                    pendingUnderscore = false;
                }

                sb.Append(char.ToLowerInvariant(c));
            }
            else if (char.IsWhiteSpace(c))
            {
                pendingUnderscore = true;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Deserializes a JSON string into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON string to deserialize.</param>
    /// <param name="option">Optional JSON serializer options.</param>
    /// <returns>The deserialized object, or the default value of <typeparamref name="T"/> if the input is null or empty.</returns>
    public static T? Deserialize<T>(this string value, JsonSerializerOptions? option = null)
    {
        if (string.IsNullOrEmpty(value)) return default;

        option ??= JsonSettings.DefaultOptions;

        return JsonSerializer.Deserialize<T?>(value, option);
    }

    /// <summary>
    /// Joins a collection of strings into a single string, using the specified separator.
    /// </summary>
    /// <param name="value">The collection of strings to join.</param>
    /// <param name="separator">The string separator to use.</param>
    /// <returns>A single string with the elements of the collection separated by the specified separator.</returns>
    public static string Join(this IEnumerable<string> value, string separator)
    {
        return string.Join(separator, value);
    }

    /// <summary>
    /// Joins a collection of strings into a single string, using the specified character as a separator.
    /// </summary>
    /// <param name="value">The collection of strings to join.</param>
    /// <param name="separator">The character separator to use.</param>
    /// <returns>A single string with the elements of the collection separated by the specified character.</returns>
    public static string Join(this IEnumerable<string> value, char separator)
    {
        return string.Join(separator, value);
    }
}