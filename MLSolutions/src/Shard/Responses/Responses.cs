namespace Shard.Responses;

public record Responses 
    (
        bool IsSuccess,
        int StatusCode,
        string? Message
    )
    : IResponses;

public record Responses<T>
    (
        bool IsSuccess,
        int StatusCode,
        string? Message,
        T? Data
    )
    : IResponses;

public record ResponesCollection
    (
        bool IsSuccess,
        int StatusCode,
        string? Message,
        IEnumerable<IResponses> Data
    )
    : IResponses;

public record ResponesCollection<T>
    (
        bool IsSuccess,
        int StatusCode,
        string? Message,
        IEnumerable<T> Data
    )
    : IResponses;

