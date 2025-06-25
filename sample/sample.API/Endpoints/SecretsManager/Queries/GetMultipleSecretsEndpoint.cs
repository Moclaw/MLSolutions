using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.SecretsManager.Queries;
using Shared.Responses;

namespace sample.API.Endpoints.SecretsManager.Queries;

[OpenApiSummary("Get multiple secrets", 
    Description = "Retrieves multiple secret values from AWS Secrets Manager")]
[OpenApiResponse(200, ResponseType = typeof(Response<GetMultipleSecretsResponse>), Description = "Secrets retrieved successfully")]
[ApiVersion("1.0")]
public class GetMultipleSecretsEndpoint(IMediator mediator)
    : SingleEndpointBase<GetMultipleSecretsQuery, GetMultipleSecretsResponse>(mediator)
{
    [HttpPost("secrets/batch")]
    public override async Task<Response<GetMultipleSecretsResponse>> HandleAsync(
        GetMultipleSecretsQuery req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
