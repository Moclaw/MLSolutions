using System.Net;

namespace Shared.Exceptions;

/// <summary>
/// Represents a business logic exception that occurs during application execution.
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BusinessException(string message) : base(message)
    {
        // Sets the HResult to indicate a Bad Request HTTP status code.
        HResult = (int)HttpStatusCode.BadRequest;
    }
}
