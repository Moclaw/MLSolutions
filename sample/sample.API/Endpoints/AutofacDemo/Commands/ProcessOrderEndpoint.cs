using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.AutofacDemo.Commands.ProcessOrder;

namespace sample.API.Endpoints.AutofacDemo.Commands;

[OpenApiSummary("Process an order using Autofac injected services", 
    Description = "Demonstrates Autofac service injection and dependency chain")]
[OpenApiResponse(200, ResponseType = typeof(Response<ProcessOrderResponse>), Description = "Order processed successfully")]
[OpenApiResponse(400, Description = "Invalid request data")]
[ApiVersion("1.0")]
public class ProcessOrderEndpoint(IMediator mediator)
    : SingleEndpointBase<ProcessOrderRequest, ProcessOrderResponse>(mediator)
{
    [HttpPost("autofac-demo/process-order")]
    public override async Task<Response<ProcessOrderResponse>> HandleAsync(
        ProcessOrderRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
