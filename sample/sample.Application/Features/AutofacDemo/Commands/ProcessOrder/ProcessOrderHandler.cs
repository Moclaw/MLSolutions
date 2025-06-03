using MediatR;
using sample.Application.Services;
using Shared.Responses;
using Shared.Utils;

namespace sample.Application.Features.AutofacDemo.Commands.ProcessOrder;

public class ProcessOrderHandler(IBusinessLogicService businessLogic)
    : IRequestHandler<ProcessOrderRequest, Response<ProcessOrderResponse>>
{
    public async Task<Response<ProcessOrderResponse>> Handle(
        ProcessOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await businessLogic.ProcessOrderAsync(request.OrderId);

            return ResponseUtils.Success(
                new ProcessOrderResponse
                {
                    Success = true,
                    Result = result,
                    Timestamp = DateTime.UtcNow
                },
                "Order processed successfully");
        }
        catch (Exception ex)
        {
            return ResponseUtils.Error<ProcessOrderResponse>(
                400,
                $"Error processing order: {ex.Message}");
        }
    }
}
