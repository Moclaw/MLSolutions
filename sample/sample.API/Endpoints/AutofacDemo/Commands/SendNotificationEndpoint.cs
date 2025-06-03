using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.AutofacDemo.Commands.SendNotification;

namespace sample.API.Endpoints.AutofacDemo.Commands;

[OpenApiSummary("Send notifications via email or SMS", 
    Description = "Demonstrates singleton service lifecycle with notification service",
    Tags = ["Autofac Demo", "Commands"])]
[OpenApiResponse(200, ResponseType = typeof(Response<NotificationResponse>), Description = "Notification sent successfully")]
[OpenApiResponse(400, Description = "Invalid request data")]
[ApiVersion("1.0")]
public class SendNotificationEndpoint(IMediator mediator)
    : SingleEndpointBase<SendNotificationRequest, NotificationResponse>(mediator)
{
    [HttpPost("autofac-demo/send-notification")]
    public override async Task<Response<NotificationResponse>> HandleAsync(
        SendNotificationRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
