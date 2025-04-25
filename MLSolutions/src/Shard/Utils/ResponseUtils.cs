using Shard.Responses;

namespace Shard.Utils;

public struct ResponseUtils
{
    public static Responses<T> Success<T>(T? data, string? message = null)
    {
        return new Responses<T>(true, 200, message, data);
    }

    public static Responses<T> Error<T>(int code, string? message = null, T? data = default)
    {
        return new Responses<T>(false, code, message, data);
    }
}
