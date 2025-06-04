using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.CacheDemo;

public class CacheDemoRequest : ICommand<CacheDemoResponse>
{
    [FromRoute(Name = "cacheType")]
    public string CacheType { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;

    public string? Key { get; set; }

    public string? Value { get; set; }
}
