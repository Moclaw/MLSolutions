using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.ProcessOrder;

public class ProcessOrderRequest : ICommand<ProcessOrderResponse>
{
    public string OrderId { get; set; } = string.Empty;
}
