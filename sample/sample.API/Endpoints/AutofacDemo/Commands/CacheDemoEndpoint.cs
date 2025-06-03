using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.AutofacDemo.Commands.CacheDemo;

namespace sample.API.Endpoints.AutofacDemo.Commands;

[OpenApiSummary("Demonstrate different cache implementations", 
    Description = "Shows how to use keyed services with Autofac for different cache types",
    Tags = ["Autofac Demo", "Commands"])]
[OpenApiResponse(200, ResponseType = typeof(Response<CacheDemoResponse>), Description = "Cache operation completed successfully")]
[OpenApiResponse(400, Description = "Invalid request data")]
[ApiVersion("1.0")]
public class CacheDemoEndpoint(IMediator mediator)
    : SingleEndpointBase<CacheDemoRequest, CacheDemoResponse>(mediator)
{
    [HttpPost("autofac-demo/cache-demo/{cacheType}")]
    public override async Task<Response<CacheDemoResponse>> HandleAsync(
        CacheDemoRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
