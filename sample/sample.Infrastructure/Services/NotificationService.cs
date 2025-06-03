using Services.Autofac.Attributes;
using System.Text.RegularExpressions;

namespace sample.Infrastructure.Services;

[ScopedService(ServiceName = "DefaultNotificationService")]
public class NotificationService : INotificationService
{
    public async Task<bool> IsEmailValidAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Basic email validation
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return await Task.FromResult(Regex.IsMatch(email, emailPattern));
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        // Simulate email sending
        await Task.Delay(100); // Simulate processing time
        
        // In a real implementation, this would integrate with an email service
        Console.WriteLine($"Email sent to {to}: Subject='{subject}', Message='{message}'");
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // Simulate SMS sending
        await Task.Delay(100); // Simulate processing time
        
        // In a real implementation, this would integrate with an SMS service
        Console.WriteLine($"SMS sent to {phoneNumber}: Message='{message}'");
    }
}
