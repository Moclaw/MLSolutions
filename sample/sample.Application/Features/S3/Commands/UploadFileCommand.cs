using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Responses;

namespace sample.Application.Features.S3.Commands;

public record UploadFileCommand : IRequest<Response<UploadFileResponse>>
{
    public IFormFile File { get; set; } = null!;
    public string? Folder { get; set; } = null!;
}

public record UploadFileResponse(
    string Key,
    string Url,
    long Size,
    string ContentType
);
