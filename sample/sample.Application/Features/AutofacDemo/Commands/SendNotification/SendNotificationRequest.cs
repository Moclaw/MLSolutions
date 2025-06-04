using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.SendNotification;

public class SendNotificationRequest : ICommand<NotificationResponse>
{
    public string Type { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? Subject { get; set; }
    
    public string? Message { get; set; }
}
