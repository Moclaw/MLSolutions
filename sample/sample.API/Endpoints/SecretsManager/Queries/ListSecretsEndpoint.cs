using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.SecretsManager.Queries;
using Shared.Responses;

namespace sample.API.Endpoints.SecretsManager.Queries;

[OpenApiSummary("List secrets", 
    Description = "Lists secrets in AWS Secrets Manager")]
[OpenApiResponse(200, ResponseType = typeof(Response<ListSecretsResponse>), Description = "Secrets listed successfully")]
[ApiVersion("1.0")]
public class ListSecretsEndpoint(IMediator mediator)
    : SingleEndpointBase<ListSecretsQuery, ListSecretsResponse>(mediator)
{
    [HttpGet("secrets")]
    public override async Task<Response<ListSecretsResponse>> HandleAsync(
        ListSecretsQuery req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
