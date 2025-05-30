using MediatR;
using MinimalAPI.Handlers;
using Services.AWS.S3.Interfaces;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.S3.Queries;

public class GetFileUrlQueryHandler : IQueryHandler<GetFileUrlQuery, GetFileUrlResponse>
{
    private readonly IS3Service _s3Service;

    public GetFileUrlQueryHandler(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<Response<GetFileUrlResponse>> Handle(GetFileUrlQuery request, CancellationToken cancellationToken)
    {
        var url = await _s3Service.GetPreSignedUrlAsync(request.Key, request.ExpiryMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(request.ExpiryMinutes);
        return new Response<GetFileUrlResponse>(
            true,
            Microsoft.AspNetCore.Http.StatusCodes.Status200OK,
            "File URL retrieved successfully",
            new GetFileUrlResponse(url, request.Key, expiresAt)
        );
    }
}
