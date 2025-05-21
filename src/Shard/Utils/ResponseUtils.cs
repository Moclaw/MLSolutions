using Shared.Responses;

namespace Shared.Utils;

/// <summary>
/// Utility class for creating standardized response objects.
/// </summary>
public struct ResponseUtils
{
    /// <summary>
    /// Creates a successful response with the specified data and optional message.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <returns>A <see cref="Responses{T}"/> object representing a successful response.</returns>
    public static Response<T> Success<T>(T? data, string? message = null)
    {
        return new Response<T>(true, 200, message, data);
    }

    public static Response Success(string? message = null, int code = 200)
    {
        return new Response(true, code, message);
    }

    /// <summary>
    /// Creates an error response with the specified status code, optional message, and optional data.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    /// <param name="code">The HTTP status code representing the error.</param>
    /// <param name="message">An optional message providing additional information about the error.</param>
    /// <param name="data">Optional data to include in the error response.</param>
    /// <returns>A <see cref="Responses{T}"/> object representing an error response.</returns>
    public static Response<T> Error<T>(int code, string? message = null, T? data = default)
    {
        return new Response<T>(false, code, message, data);
    }

    /// <summary>
    /// Creates an error response with the specified status code and optional message.
    /// </summary>
    /// <param name="code">The HTTP status code representing the error.</param>
    /// <param name="message">An optional message providing additional information about the error.</param>
    /// <returns>
    /// A <see cref="Response"/> object representing an error response.
    /// </returns>
    public static Response Error(string? message = null, int code = 400)
    {
        return new Response(false, code, message);
    }
}
