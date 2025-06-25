using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Endpoints;
using sample.Application.Features.SecretsManager.Queries;
using Shared.Responses;

namespace sample.API.Endpoints.SecretsManager.Queries;

[OpenApiSummary("Get secret value", 
    Description = "Retrieves a secret value from AWS Secrets Manager")]
[OpenApiResponse(200, ResponseType = typeof(Response<GetSecretResponse>), Description = "Secret retrieved successfully")]
[OpenApiResponse(404, Description = "Secret not found")]
[ApiVersion("1.0")]
public class GetSecretEndpoint(IMediator mediator)
    : SingleEndpointBase<GetSecretQuery, GetSecretResponse>(mediator)
{
    [HttpGet("secrets/{secretName}")]
    public override async Task<Response<GetSecretResponse>> HandleAsync(
        GetSecretQuery req,
        CancellationToken ct
    )
    {
        return await _mediator.Send(req, ct);
    }
}
