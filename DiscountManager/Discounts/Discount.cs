namespace DiscountManager.Discounts;

public class Discount
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public DiscountType Type { get; set; }
    public int Priority { get; set; }
}
