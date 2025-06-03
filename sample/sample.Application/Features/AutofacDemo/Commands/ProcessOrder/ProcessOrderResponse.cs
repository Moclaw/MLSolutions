namespace sample.Application.Features.AutofacDemo.Commands.ProcessOrder;

public record ProcessOrderResponse
{
    public bool Success { get; init; }
    public string Result { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
