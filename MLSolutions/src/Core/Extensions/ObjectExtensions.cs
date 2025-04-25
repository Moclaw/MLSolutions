using System.Text;
using System.Text.Json;

namespace MLSolutions;

public static class ObjectExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="orignal"></param>
    /// <param name="encoding">Default: Encoding.UTF8</param>
    /// <returns></returns>
    public static string ConvertToString(this byte[] orignal, Encoding? encoding = null)
    {
        if (orignal == null || orignal.Length <= 0) return string.Empty;

        encoding ??= Encoding.UTF8;

        return encoding.GetString(orignal);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="orignal"></param>
    /// <param name="encoding">Default: Encoding.UTF8</param>
    /// <returns></returns>
    public static byte[] ToByteArray(this string orignal, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(orignal)) return Array.Empty<byte>();

        encoding ??= Encoding.UTF8;

        return encoding.GetBytes(orignal);
    }

    public static string Serialize<T>(this T data, JsonSerializerOptions? option = null)
    {
        option ??= Settings.DefaultOptions;

        return JsonSerializer.Serialize(data, option);
    }
}
