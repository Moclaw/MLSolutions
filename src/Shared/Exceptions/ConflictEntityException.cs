using System.Net;

namespace Shared.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a conflict occurs with an entity.
/// </summary>
public class ConflictEntityException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictEntityException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConflictEntityException(string message) : base(message)
    {
        HResult = (int)HttpStatusCode.Conflict;
    }
}
