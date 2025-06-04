using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;
using Shared.Responses;

namespace sample.Application.Features.S3.Commands;

public record DeleteFileCommand : ICommand<DeleteFileResponse>
{
    [FromRoute]
    public string Key { get; set; }
}

public record DeleteFileResponse(
    string Key,
    bool Success,
    string Message
);
