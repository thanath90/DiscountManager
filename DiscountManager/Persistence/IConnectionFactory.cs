using System.Data;

namespace DiscountManager.Persistence;

public interface IConnectionFactory
{
    IDbConnection Create();
}
