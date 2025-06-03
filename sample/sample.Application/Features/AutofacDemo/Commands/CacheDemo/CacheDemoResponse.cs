namespace sample.Application.Features.AutofacDemo.Commands.CacheDemo;

public record CacheDemoResponse
{
    public bool Success { get; init; }
    public string Operation { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string CacheType { get; init; } = string.Empty;
}
