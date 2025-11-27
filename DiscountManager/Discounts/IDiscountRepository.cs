namespace DiscountManager.Discounts;

public interface IDiscountRepository
{
    Task<IEnumerable<Discount>> GetAsync();
    Task<Discount?> Get(int id);
    Task<Discount?> Get(string name);
    Task<Discount> Create(Discount discount);
    Task<Discount> Update(Discount discount);
    Task Delete(int id);
    Task<bool> PriorityExists(int id, int priority);
}
