using MediatR;
using Services.AWS.S3.Interfaces;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.S3.Commands;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Response<UploadFileResponse>>
{
    private readonly IS3Service _s3Service;

    public UploadFileCommandHandler(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<Response<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;
        var folder = string.IsNullOrEmpty(request.Folder) ? "uploads" : request.Folder;
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var key = $"{folder}/{fileName}";

        using var stream = file.OpenReadStream();
        await _s3Service.UploadFileAsync(key, stream, file.ContentType);

        var url = await _s3Service.GetPreSignedUrlAsync(key, 60);

        var response = new UploadFileResponse(
            Key: key,
            Url: url,
            Size: file.Length,
            ContentType: file.ContentType
        );

        return ResponseUtils.Success(response, "File uploaded successfully");

    }
}
