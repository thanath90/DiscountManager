
using Dapper;
using DiscountManager.Persistence;

namespace DiscountManager.Discounts;

public class DiscountRepository : IDiscountRepository
{
    private const string CreateQuery = @"INSERT INTO Discount (Name, Type, Priority) VALUES (@Name, @Type, @Priority); SELECT last_insert_rowid();";
    private const string GetByNameQuery = @"SELECT Id, Name, Type, Priority FROM Discount WHERE Name = @Name;";
    private const string GetByIdQuery = @"SELECT Id, Name, Type, Priority FROM Discount WHERE Id = @Id;";
    private const string GetAllQuery = @"SELECT Id, Name, Type, Priority FROM Discount;";
    private const string UpdateQuery = @"UPDATE Discount SET Name = @Name, Type = @Type, Priority = @Priority WHERE Id = @Id;";
    private const string DeleteQuery = @"DELETE FROM Discount WHERE Id = @Id;";

    private readonly IConnectionFactory _connectionFactory;

    public DiscountRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Discount> Create(Discount discount)
    {
        var existingDiscount = await Get(discount.Name);
        if (existingDiscount is null)
        {
            using var connection = _connectionFactory.Create();
            var id = await connection.ExecuteScalarAsync<int>(CreateQuery, new { discount.Name, discount.Type, discount.Priority });
            discount.Id = id;
            return discount;
        }

        return existingDiscount;
    }

    public async Task Delete(int id)
    {
        using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(DeleteQuery, new { Id = id });
    }

    public async Task<Discount?> Get(int id)
    {
        using var connection = _connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<Discount?>(GetByIdQuery, new { Id = id });
    }

    public async Task<Discount?> Get(string name)
    {
        using var connection = _connectionFactory.Create();
        return await connection.QueryFirstOrDefaultAsync<Discount?>(GetByNameQuery, new { Name = name });
    }

    public async Task<IEnumerable<Discount>> GetAsync()
    {
        using var connection = _connectionFactory.Create();
        var discounts = await connection.QueryAsync<Discount>(GetAllQuery);
        return discounts;
    }

    public async Task<bool> PriorityExists(int id, int priority)
    {
        using var connection = _connectionFactory.Create();
        var query = "SELECT COUNT(1) FROM Discount WHERE id <> @Id AND Priority = @Priority;";
        var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id, Priority = priority });
        return count > 0;
    }

    public async Task<Discount> Update(Discount discount)
    {
        using var connection = _connectionFactory.Create();
        return await connection.QuerySingleAsync<Discount>(UpdateQuery + GetByIdQuery, new { discount.Name, discount.Type, discount.Priority, discount.Id });
    }
}
