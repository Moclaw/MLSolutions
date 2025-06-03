using MediatR;
using MinimalAPI.Attributes;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.ProcessOrder;

public class ProcessOrderRequest : IRequest<Response<ProcessOrderResponse>>
{
    [FromBody]
    public string OrderId { get; set; } = string.Empty;
}
