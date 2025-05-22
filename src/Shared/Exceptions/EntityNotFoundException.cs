using System.Net;

namespace Shared.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EntityNotFoundException(string message) : base(message)
    {
        // Sets the HResult to indicate an HTTP status code of No Content (204).
        HResult = (int)HttpStatusCode.NoContent;
    }
}
