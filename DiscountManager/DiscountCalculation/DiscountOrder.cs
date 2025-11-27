using DiscountManager.Discounts;

namespace DiscountManager.DiscountCalculation;

public record DiscountOrder(decimal InitialAmount, decimal AmountAfterDiscount, IEnumerable<DiscountAnalysis> Analysis);

public record DiscountAnalysis(Discount Discount, decimal DiscountAmount);

