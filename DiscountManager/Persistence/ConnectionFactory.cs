using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace DiscountManager.Persistence;

public class ConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public ConnectionFactory()
    {
        var basePath = AppContext.BaseDirectory;
        var dbPath = Path.Combine(basePath, "appdb.sqlite");

        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = Create();
        const string sql = @"
            CREATE TABLE IF NOT EXISTS Discount (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Priority INTEGER NOT NULL
            );
        ";
        conn.ExecuteAsync(sql);
        var count = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Discount;");

        if (count == 0)
        {
            conn.ExecuteAsync(@"
                INSERT INTO Discount (Name, Type, Priority) VALUES
                ('Price List Discount', 0, 1),
                ('Promotion', 0, 2);");
        }
    }

    public IDbConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
