using MediatR;
using MinimalAPI.Attributes;
using sample.Application.Features.AutofacDemo.Queries.ExportData;

namespace sample.API.Endpoints.AutofacDemo.Queries;

[OpenApiSummary("Export sample data to JSON", 
    Description = "Demonstrates transient service lifecycle with data export service",
    Tags = ["Autofac Demo", "Queries"])]
[OpenApiResponse(200, ResponseType = typeof(Response<ExportDataResponse>), Description = "Data exported successfully")]
[OpenApiResponse(400, Description = "Export failed")]
[ApiVersion("1.0")]
public class ExportSampleDataEndpoint(IMediator mediator)
    : SingleEndpointBase<ExportSampleDataRequest, ExportDataResponse>(mediator)
{
    [HttpGet("autofac-demo/export-sample-data")]
    public override async Task<Response<ExportDataResponse>> HandleAsync(
        ExportSampleDataRequest req,
        CancellationToken ct
    ) => await _mediator.Send(req, ct);
}
