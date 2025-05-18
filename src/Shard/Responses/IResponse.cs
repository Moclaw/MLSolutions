namespace Shard.Responses
{
    /// <summary>
    /// Represents a response interface that provides details about the success, status, and message of an operation.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Gets the HTTP status code associated with the response.
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// Gets an optional message providing additional information about the response.
        /// </summary>
        string? Message { get; }
    }
}
