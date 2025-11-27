using DiscountManager.Discounts;

namespace DiscountManager.DiscountCalculation;

public class DiscountCalculationService : IDiscountCalculationService
{
    private readonly IDiscountRepository _discountRepository;

    public DiscountCalculationService(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
    }

    public async Task<DiscountOrder> Calculate(decimal orderAmount, Dictionary<string, decimal> availableDiscounts)
    {
        var discounts = await GetAvailableDiscounts(availableDiscounts.Keys);
        return ApplyDiscounts(orderAmount, discounts, availableDiscounts);
    }

    private async Task<IEnumerable<Discount>> GetAvailableDiscounts(IEnumerable<string> discountNames)
    {
        var discounts = new List<Discount>();
        foreach (var name in discountNames)
        {
            var discount = await _discountRepository.Get(name);
            if (discount is not null)
            {
                discounts.Add(discount);
            }
        }

        return discounts;
    }

    private DiscountOrder ApplyDiscounts(decimal orderAmount, IEnumerable<Discount> discounts, Dictionary<string, decimal> availableDiscounts)
    {
        var analysis = new List<DiscountAnalysis>();
        var currentAmount = orderAmount;
        foreach (var discount in discounts.OrderBy(d => d.Priority))
        {
            if (availableDiscounts.TryGetValue(discount.Name, out var discountAmountToApply))
            {
                var discountAmount = GetDiscountAmount(currentAmount, discount.Type, discountAmountToApply);
                currentAmount = currentAmount - discountAmount;
                analysis.Add(new DiscountAnalysis(discount, discountAmount));
            }
        }

        return new DiscountOrder(orderAmount, currentAmount, analysis);
    }

    private decimal GetDiscountAmount(decimal ammount, DiscountType discountType, decimal discountAmount)
    {
        if (DiscountType.Percentage == discountType)
        {
            return ammount * (discountAmount / 100);
        }
        else if (DiscountType.FixedAmount == discountType)
        {
            return discountAmount;
        }
        
        throw new Exception("Unsupported discount type");
    }
}
