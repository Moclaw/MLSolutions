using MediatR;
using Services.AWS.S3.Interfaces;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.S3.Queries;

public class GetFileUrlQueryHandler : IRequestHandler<GetFileUrlQuery, Response<GetFileUrlResponse>>
{
    private readonly IS3Service _s3Service;

    public GetFileUrlQueryHandler(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<Response<GetFileUrlResponse>> Handle(GetFileUrlQuery request, CancellationToken cancellationToken)
    {
        var exists = await _s3Service.FileExistsAsync(request.Key);
        if (!exists)
        {
            var notFoundResponse = new GetFileUrlResponse(
                Url: null,
                Key: request.Key,
                DateTime.UtcNow.AddMinutes(request.ExpiryMinutes)
            );
            return ResponseUtils.Error(404, "File not found", notFoundResponse);
        }

        var url = await _s3Service.GetPreSignedUrlAsync(request.Key, request.ExpiryMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(request.ExpiryMinutes);

        var response = new GetFileUrlResponse(
            Url: url,
            Key: request.Key,
            ExpiresAt: expiresAt
        );

        return ResponseUtils.Success(response, "Files retrieved successfully");

    }
}
