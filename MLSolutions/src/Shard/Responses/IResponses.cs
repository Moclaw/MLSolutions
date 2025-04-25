namespace Shard.Responses
{
    public interface IResponses
    {
        bool IsSuccess { get; }

        int StatusCode { get; }
        string? Message { get; }
    }
}
