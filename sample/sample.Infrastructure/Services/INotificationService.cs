namespace sample.Infrastructure.Services;

public interface INotificationService
{
    Task<bool> IsEmailValidAsync(string email);
    Task SendEmailAsync(string to, string subject, string message);
    Task SendSmsAsync(string phoneNumber, string message);
}
