using MediatR;
using Shared.Responses;

namespace sample.Application.Features.S3.Commands;

public record DeleteFileCommand : IRequest<Response<DeleteFileResponse>>
{
    public string Key { get; set; }
}

public record DeleteFileResponse(
    string Key,
    bool Success,
    string Message
);
