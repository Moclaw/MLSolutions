using Services.Autofac.Attributes;

namespace sample.Infrastructure.Services;

[ScopedService(ServiceName = "SmsNotificationService")]
public class SmsNotificationService : INotificationService
{
    public async Task<bool> IsEmailValidAsync(string email)
    {
        throw new NotSupportedException("Email validation is not supported by the SMS notification service");
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        throw new NotSupportedException("Email is not supported by the SMS notification service");
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        await Task.Delay(80); // Simulate SMS processing
        Console.WriteLine($"[SMS SERVICE] Sent to {phoneNumber}: Message='{message}'");
    }
}
