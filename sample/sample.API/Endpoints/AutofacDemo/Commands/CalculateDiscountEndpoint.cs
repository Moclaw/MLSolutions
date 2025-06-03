using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.AutofacDemo.Commands.CalculateDiscount;

namespace sample.API.Endpoints.AutofacDemo.Commands;

[OpenApiSummary("Calculate discount for customer using Autofac services", 
    Description = "Shows how services can depend on other services through Autofac",
    Tags = ["Autofac Demo", "Commands"])]
[OpenApiResponse(200, ResponseType = typeof(Response<CalculateDiscountResponse>), Description = "Discount calculated successfully")]
[OpenApiResponse(400, Description = "Invalid request data")]
[ApiVersion("1.0")]
public class CalculateDiscountEndpoint(IMediator mediator)
    : SingleEndpointBase<CalculateDiscountRequest, CalculateDiscountResponse>(mediator)
{
    [HttpPost("autofac-demo/calculate-discount")]
    public override async Task<Response<CalculateDiscountResponse>> HandleAsync(
        CalculateDiscountRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
