using MediatR;
using sample.Application.Services;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.AutofacDemo.Commands.CalculateDiscount;

public class CalculateDiscountHandler(IBusinessLogicService businessLogic) 
    : IRequestHandler<CalculateDiscountRequest, Response<CalculateDiscountResponse>>
{
    public async Task<Response<CalculateDiscountResponse>> Handle(CalculateDiscountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var discount = await businessLogic.CalculateDiscountAsync(request.Amount, request.CustomerType);
            var finalAmount = request.Amount - discount;
            
            return ResponseUtils.Success(new CalculateDiscountResponse
            {
                OriginalAmount = request.Amount,
                Discount = discount,
                FinalAmount = finalAmount,
                CustomerType = request.CustomerType,
                Timestamp = DateTime.UtcNow
            }, "Discount calculated successfully");
        }
        catch (Exception ex)
        {
            return ResponseUtils.Error<CalculateDiscountResponse>(400, $"Error calculating discount: {ex.Message}");
        }
    }
}
