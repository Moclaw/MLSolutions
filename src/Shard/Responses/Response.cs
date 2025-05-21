namespace Shared.Responses;

/// <summary>
/// Represents a basic response with success status, HTTP status code, and an optional message.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation was successful.</param>
/// <param name="StatusCode">The HTTP status code associated with the response.</param>
/// <param name="Message">An optional message providing additional information about the response.</param>
public record Response(bool IsSuccess, int StatusCode, string? Message) : IResponse;

/// <summary>
/// Represents a generic response with success status, HTTP status code, an optional message, and additional data.
/// </summary>
/// <typeparam name="T">The type of the data included in the response.</typeparam>
/// <param name="IsSuccess">Indicates whether the operation was successful.</param>
/// <param name="StatusCode">The HTTP status code associated with the response.</param>
/// <param name="Message">An optional message providing additional information about the response.</param>
/// <param name="Data">The data included in the response.</param>
public record Response<T>(bool IsSuccess, int StatusCode, string? Message, T? Data) : IResponse;

/// <summary>
/// Represents a collection response with success status, HTTP status code, an optional message, and a collection of responses.
/// </summary>
/// <param name="IsSuccess">Indicates whether the operation was successful.</param>
/// <param name="StatusCode">The HTTP status code associated with the response.</param>
/// <param name="Message">An optional message providing additional information about the response.</param>
/// <param name="Data">A collection of responses included in the response.</param>
public record ResponesCollection(
    bool IsSuccess,
    int StatusCode,
    string? Message,
    IEnumerable<IResponse> Data
) : IResponse;

/// <summary>
/// Represents a generic collection response with success status, HTTP status code, an optional message, and a collection of data.
/// </summary>
/// <typeparam name="T">The type of the data included in the collection.</typeparam>
/// <param name="IsSuccess">Indicates whether the operation was successful.</param>
/// <param name="StatusCode">The HTTP status code associated with the response.</param>
/// <param name="Message">An optional message providing additional information about the response.</param>
/// <param name="Data">A collection of data included in the response.</param>
public record ResponesCollection<T>(
    bool IsSuccess,
    int StatusCode,
    string? Message,
    IEnumerable<T> Data
) : IResponse;
