using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers;
using Shared.Responses;

namespace sample.Application.Features.S3.Queries;

public record GetFileUrlQuery : IQueryRequest<GetFileUrlResponse>
{

    [FromRoute]
    public string Key { get; set; } = null!;
    public int ExpiryMinutes { get; set; } = 60; // Default to 60 minutes

}

public record GetFileUrlResponse(
    string Url,
    string Key,
    DateTime ExpiresAt
);
