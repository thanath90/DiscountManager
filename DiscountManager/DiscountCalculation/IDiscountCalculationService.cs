namespace DiscountManager.DiscountCalculation;

public interface IDiscountCalculationService
{
    Task<DiscountOrder> Calculate(decimal orderAmount, Dictionary<string, decimal> availableDiscounts);
}
