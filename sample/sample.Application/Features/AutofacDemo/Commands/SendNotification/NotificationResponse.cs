namespace sample.Application.Features.AutofacDemo.Commands.SendNotification;

public record NotificationResponse
{
    public bool Success { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Recipient { get; init; } = string.Empty;
}
