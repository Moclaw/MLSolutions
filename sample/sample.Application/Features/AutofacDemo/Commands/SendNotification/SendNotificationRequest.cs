using MediatR;
using MinimalAPI.Attributes;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.SendNotification;

public class SendNotificationRequest : IRequest<Response<NotificationResponse>>
{
    [FromBody]
    public string Type { get; set; } = string.Empty;
    
    [FromBody]
    public string? Email { get; set; }
    
    [FromBody]
    public string? PhoneNumber { get; set; }
    
    [FromBody]
    public string? Subject { get; set; }
    
    [FromBody]
    public string? Message { get; set; }
}
