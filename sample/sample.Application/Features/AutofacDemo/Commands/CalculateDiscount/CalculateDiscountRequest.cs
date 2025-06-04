using MediatR;
using MinimalAPI.Attributes;
using MinimalAPI.Handlers.Command;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.CalculateDiscount;

public class CalculateDiscountRequest : ICommand<CalculateDiscountResponse>
{
    public decimal Amount { get; set; }
    
    public string CustomerType { get; set; } = string.Empty;
}
