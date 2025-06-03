using Microsoft.Extensions.Logging;
using Services.Autofac.Attributes;
using sample.Infrastructure.Services;

namespace sample.Application.Services;

/// <summary>
/// Service for handling business logic operations
/// </summary>
public interface IBusinessLogicService
{
    Task<string> ProcessOrderAsync(string orderId);
    Task<decimal> CalculateDiscountAsync(decimal amount, string customerType);
}

/// <summary>
/// Default implementation demonstrating Autofac attribute registration
/// </summary>
[ScopedService]
public class BusinessLogicService : IBusinessLogicService
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<BusinessLogicService> _logger;

    public BusinessLogicService(
        INotificationService notificationService,
        ILogger<BusinessLogicService> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<string> ProcessOrderAsync(string orderId)
    {
        _logger.LogInformation("Processing order: {OrderId}", orderId);
        
        // Simulate business logic
        await Task.Delay(200);
        
        var result = $"Order {orderId} processed successfully at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        
        // Send notification
        await _notificationService.SendEmailAsync(
            "admin@example.com", 
            "Order Processed", 
            result);
            
        _logger.LogInformation("Order processing completed: {OrderId}", orderId);
        return result;
    }

    public async Task<decimal> CalculateDiscountAsync(decimal amount, string customerType)
    {
        _logger.LogInformation("Calculating discount for amount {Amount} and customer type {CustomerType}", 
            amount, customerType);
            
        await Task.Delay(50);
        
        var discountPercentage = customerType.ToLower() switch
        {
            "premium" => 0.15m,
            "gold" => 0.10m,
            "silver" => 0.05m,
            _ => 0.02m
        };
        
        var discount = amount * discountPercentage;
        _logger.LogInformation("Calculated discount: {Discount} ({Percentage}%)", 
            discount, discountPercentage * 100);
            
        return discount;
    }
}
