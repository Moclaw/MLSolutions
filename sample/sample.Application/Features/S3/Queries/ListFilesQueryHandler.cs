using MediatR;
using Services.AWS.S3.Interfaces;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.S3.Queries;

public class ListFilesQueryHandler : IRequestHandler<ListFilesQuery, Response<ListFilesResponse>>
{
    private readonly IS3Service _s3Service;

    public ListFilesQueryHandler(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<Response<ListFilesResponse>> Handle(ListFilesQuery request, CancellationToken cancellationToken)
    {
        var objects = await _s3Service.ListObjectsAsync(request.Prefix, request.Recursive);

        var files = objects.Select(obj => new FileInfo(
            Key: obj.Key,
            Size: obj.Size.GetValueOrDefault(),
            LastModified: obj.LastModified ?? DateTime.Now,
            ETag: obj.ETag
        )).ToList();

        var response = new ListFilesResponse(Files: files);
        return ResponseUtils.Success(response, "Files retrieved successfully");
    }
}
