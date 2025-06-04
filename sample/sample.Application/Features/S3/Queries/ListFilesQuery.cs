using MediatR;
using MinimalAPI.Handlers.Command;
using Shared.Responses;

namespace sample.Application.Features.S3.Queries;

public record ListFilesQuery(

) : ICommand<ListFilesResponse>
{
   public string Prefix { get; set; } = string.Empty;
    public bool Recursive { get; set; } = false;
}

public record ListFilesResponse(
    List<FileInfo> Files
);

public record FileInfo(
    string Key,
    long Size,
    DateTime LastModified,
    string ETag
);
