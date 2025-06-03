namespace sample.Application.Features.AutofacDemo.Queries.ExportData;

public record ExportDataResponse
{
    public string Data { get; init; } = string.Empty;
    public DateTime ExportedAt { get; init; }
    public string Format { get; init; } = string.Empty;
    public int ItemCount { get; init; }
}
