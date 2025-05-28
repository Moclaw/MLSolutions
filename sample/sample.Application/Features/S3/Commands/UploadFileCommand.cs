using MediatR;
using Microsoft.AspNetCore.Http;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;
using System.ComponentModel.DataAnnotations;

namespace sample.Application.Features.S3.Commands;

public class UploadFileCommand : ICommand<UploadFileResponse>
{
    [FromForm]
    [Required]
    public IFormFile File { get; set; } = null!;
    
    [FromQuery]
    public string? Folder { get; set; }
}

public record UploadFileResponse(
    string Key,
    string Url,
    long Size,
    string ContentType
);
