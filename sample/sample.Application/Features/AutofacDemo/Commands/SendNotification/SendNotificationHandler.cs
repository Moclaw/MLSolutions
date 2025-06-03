using MediatR;
using sample.Infrastructure.Services;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.AutofacDemo.Commands.SendNotification;

public class SendNotificationHandler(INotificationService notificationService) 
    : IRequestHandler<SendNotificationRequest, Response<NotificationResponse>>
{
    public async Task<Response<NotificationResponse>> Handle(SendNotificationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            switch (request.Type.ToLower())
            {                case "email":
                    if (string.IsNullOrEmpty(request.Email))
                        return ResponseUtils.Error<NotificationResponse>(400, "Email is required for email notifications");
                    
                    var isValid = await notificationService.IsEmailValidAsync(request.Email);
                    if (!isValid)
                        return ResponseUtils.Error<NotificationResponse>(400, "Invalid email address");
                    
                    await notificationService.SendEmailAsync(request.Email, request.Subject ?? "Test", request.Message ?? "Test message");
                    return ResponseUtils.Success(new NotificationResponse
                    {
                        Success = true,
                        Type = "Email",
                        Recipient = request.Email
                    });

                case "sms":
                    if (string.IsNullOrEmpty(request.PhoneNumber))
                        return ResponseUtils.Error<NotificationResponse>(400, "Phone number is required for SMS notifications");
                    
                    await notificationService.SendSmsAsync(request.PhoneNumber, request.Message ?? "Test SMS");
                    return ResponseUtils.Success(new NotificationResponse
                    {
                        Success = true,
                        Type = "SMS",
                        Recipient = request.PhoneNumber
                    });

                default:
                    return ResponseUtils.Error<NotificationResponse>(400, $"Invalid notification type: {request.Type}");
            }        }
        catch (Exception ex)
        {
            return ResponseUtils.Error<NotificationResponse>(500, $"Error sending notification: {ex.Message}");
        }
    }
}
