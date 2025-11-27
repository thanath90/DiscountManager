namespace DiscountManager.DiscountCalculation;

public record DiscountCalculationRequest(decimal OrderAmount,
    Dictionary<string, decimal> AvailableDiscounts
);
