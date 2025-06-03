using MediatR;
using MinimalAPI.Attributes;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.CacheDemo;

public class CacheDemoRequest : IRequest<Response<CacheDemoResponse>>
{    [FromRoute(Name = "cacheType")]
    public string CacheType { get; set; } = string.Empty;
    
    [FromBody]
    public string Operation { get; set; } = string.Empty;
    
    [FromBody]
    public string? Key { get; set; }
    
    [FromBody]
    public string? Value { get; set; }
}
