

using Shard.Settings;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MLSolutions;

public static class StringExtensions
{
    public static string ToNormalize(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Optimize by using a single loop rather than LINQ and Regex.
        // Only letters, digits, and white-space are allowed.
        // Convert letters to lowercase and replace groups of whitespace with a single underscore.
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
                // Mark that an underscore should be added when a valid character is encountered
                pendingUnderscore = true;
            }
            // Ignore non-alphanumeric, non-whitespace characters.
        }

        // The algorithm avoids adding underscores at the beginning or end.
        return sb.ToString();
    }

    public static T? Deserialize<T>(this string value, JsonSerializerOptions? option = null)
    {
        if (string.IsNullOrEmpty(value)) return default;

        option ??= JsonSettings.DefaultOptions;

        return JsonSerializer.Deserialize<T?>(value, option);
    }

    public static string Join(this IEnumerable<string> value, string separator)
    {
        return string.Join(separator, value);
    }

    public static string Join(this IEnumerable<string> value, char separator)
    {
        return string.Join(separator, value);
    }
}
