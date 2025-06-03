using MediatR;
using MinimalAPI.Attributes;
using Shared.Responses;

namespace sample.Application.Features.AutofacDemo.Commands.CalculateDiscount;

public class CalculateDiscountRequest : IRequest<Response<CalculateDiscountResponse>>
{
    [FromBody]
    public decimal Amount { get; set; }
    
    [FromBody]
    public string CustomerType { get; set; } = string.Empty;
}
