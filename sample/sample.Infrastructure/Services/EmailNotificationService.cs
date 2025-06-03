using Services.Autofac.Attributes;

namespace sample.Infrastructure.Services;

[ScopedService(ServiceName = "EmailNotificationService")]
public class EmailNotificationService : INotificationService
{
    public async Task<bool> IsEmailValidAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Enhanced email validation for email service
        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return await Task.FromResult(System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern));
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        await Task.Delay(150); // Simulate email processing
        Console.WriteLine($"[EMAIL SERVICE] Sent to {to}: Subject='{subject}', Message='{message}'");
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        throw new NotSupportedException("SMS is not supported by the Email notification service");
    }
}
