using Shared.Responses;

namespace Shared.Utils;

/// <summary>
/// Utility class for creating standardized response objects.
/// </summary>
public struct ResponseUtils
{
    // ===== SUCCESS RESPONSES =====

    /// <summary>
    /// Creates a successful response with the specified data and optional message.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <returns>A <see cref="Response{T}"/> object representing a successful response.</returns>
    public static Response<T> Success<T>(T? data, string? message = null)
    {
        return new Response<T>(true, 200, message, data);
    }

    /// <summary>
    /// Creates a successful response with the specified message and status code.
    /// </summary>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <param name="code">The HTTP status code representing the success.</param>
    /// <returns>A <see cref="Response"/> object representing a successful response.</returns>
    public static Response Success(string? message = null, int code = 200)
    {
        return new Response(true, code, message);
    }

    /// <summary>
    /// Creates a successful collection response with the specified data, status code, message, and pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    /// <param name="data">The collection of data to include in the response.</param>
    /// <param name="statusCode">The HTTP status code representing the success.</param>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <param name="pagination">Optional pagination information for the response.</param>
    /// <returns>A <see cref="ResponseCollection{T}"/> object representing a successful collection response.</returns>
    public static ResponseCollection<T> Success<T>(
        IEnumerable<T> data,
        int statusCode = 200,
        string? message = null,
        Pagination? pagination = null
    )
    {
        return new ResponseCollection<T>(true, statusCode, message, data, pagination);
    }

    /// <summary>
    /// Creates a successful collection response with the specified data, status code, message, and pagination for non-generic responses.
    /// </summary>
    /// <param name="data">The collection of data to include in the response.</param>
    /// <param name="statusCode">The HTTP status code representing the success.</param>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <param name="pagination">Optional pagination information for the response.</param>
    /// <returns>A <see cref="ResponseCollection"/> object representing a successful collection response.</returns>
    public static ResponseCollection Success(
        IEnumerable<IResponse> data,
        int statusCode = 200,
        string? message = null,
        Pagination? pagination = null
    )
    {
        return new ResponseCollection(true, statusCode, message, data, pagination);
    }

    // ===== ERROR RESPONSES =====

    /// <summary>
    /// Creates an error response with the specified status code, optional message, and optional data.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    /// <param name="code">The HTTP status code representing the error.</param>
    /// <param name="message">An optional message providing additional information about the error.</param>
    /// <param name="data">Optional data to include in the error response.</param>
    /// <returns>A <see cref="Response{T}"/> object representing an error response.</returns>
    public static Response<T> Error<T>(int code, string? message = null, T? data = default)
    {
        return new Response<T>(false, code, message, data);
    }

    /// <summary>
    /// Creates an error response with the specified status code and optional message.
    /// </summary>
    /// <param name="code">The HTTP status code representing the error.</param>
    /// <param name="message">An optional message providing additional information about the error.</param>
    /// <returns>A <see cref="Response"/> object representing an error response.</returns>
    public static Response Error(string? message = null, int code = 400)
    {
        return new Response(false, code, message);
    }

    /// <summary>
    /// Creates an error collection response with the specified data, status code, message, and pagination for non-generic responses.
    /// </summary>
    /// <param name="data">The collection of data to include in the error response.</param>
    /// <param name="statusCode">The HTTP status code representing the error.</param>
    /// <param name="message">An optional message providing additional information about the error.</param>
    /// <param name="pagination">Optional pagination information for the error response.</param>
    /// <returns>A <see cref="ResponseCollection"/> object representing an error collection response.</returns>
    public static ResponseCollection Error(
        IEnumerable<IResponse> data,
        int statusCode = 400,
        string? message = null,
        Pagination? pagination = null
    )
    {
        return new ResponseCollection(false, statusCode, message, data, pagination);
    }

    /// <summary>
    /// Creates an error collection response with the specified data, status code, message, and pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the error response.</typeparam>
    /// <param name="data">The collection of data to include in the error response.</param>
    /// <param name="statusCode">The HTTP status code representing the error.</param>
    /// <param name="message">An optional message providing additional information about the error.</param>
    /// <param name="pagination">Optional pagination information for the error response.</param>
    /// <returns>A <see cref="ResponseCollection{T}"/> object representing an error collection response.</returns>
    public static ResponseCollection<T> Error<T>(
        IEnumerable<T> data,
        int statusCode = 400,
        string? message = null,
        Pagination? pagination = null
    )
    {
        return new ResponseCollection<T>(false, statusCode, message, data, pagination);
    }

    // ===== NOT FOUND RESPONSES =====

    /// <summary>
    /// Creates a not found response with the specified message and optional data.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <param name="data">Optional data to include in the response.</param>
    /// <returns>A <see cref="Response{T}"/> object representing a not found response.</returns>
    public static Response<T> NotFound<T>(string? message = null, T? data = default)
    {
        return new Response<T>(false, 204, message, data);
    }

    /// <summary>
    /// Creates a not found response with the specified message.
    /// </summary>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <returns>A <see cref="Response"/> object representing a not found response.</returns>
    public static Response NotFound(string? message = null)
    {
        return new Response(false, 204, message);
    }

    /// <summary>
    /// Creates a not found collection response with the specified message and data.
    /// </summary>
    /// <typeparam name="T">The type of the data included in the response.</typeparam>
    /// <param name="message">An optional message providing additional information about the response.</param>
    /// <param name="data">Optional data to include in the response.</param>
    /// <returns>A <see cref="ResponseCollection{T}"/> object representing a not found response.</returns>
    public static ResponseCollection<T> NotFound<T>(
        string? message = null,
        IEnumerable<T>? data = default
    )
    {
        return new ResponseCollection<T>(false, 204, message, data ?? Array.Empty<T>());
    }
}
    