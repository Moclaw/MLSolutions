namespace sample.Application.Features.AutofacDemo.Commands.CalculateDiscount;

public record CalculateDiscountResponse
{
    public decimal OriginalAmount { get; init; }
    public decimal Discount { get; init; }
    public decimal FinalAmount { get; init; }
    public string CustomerType { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
