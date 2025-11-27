using Dapper;
using DiscountManager.Persistence;
using System.Data;

namespace DiscountManager.Tests;

public class ConnectionFactoryTests
{
    private string CreateTempFolder()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public void Constructor_ShouldCreateDatabaseFile()
    {
        // Arrange
        var tempDir = CreateTempFolder();
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir);

        // Act
        var factory = new ConnectionFactory();

        var expectedPath = Path.Combine(tempDir, "appdb.sqlite");

        // Assert
        Assert.True(File.Exists(expectedPath), "Database file should be created automatically.");
    }

    [Fact]
    public void Create_ShouldReturnOpenConnection()
    {
        // Arrange
        var tempDir = CreateTempFolder();
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir);
        var factory = new ConnectionFactory();

        // Act
        using var conn = factory.Create();

        // Assert
        Assert.NotNull(conn);
        Assert.Equal(ConnectionState.Open, conn.State);
    }

    [Fact]
    public void InitializeDatabase_ShouldCreateDiscountTable()
    {
        // Arrange
        var tempDir = CreateTempFolder();
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir);
        var factory = new ConnectionFactory();
        using var conn = factory.Create();

        // Act
        var tables = conn.Query<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name='Discount';");

        // Assert
        Assert.Single(tables);
    }

    [Fact]
    public void InitializeDatabase_ShouldInsertSeedData_WhenEmpty()
    {
        // Arrange
        var tempDir = CreateTempFolder();
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir);
        var factory = new ConnectionFactory();
        using var conn = factory.Create();

        // Act
        var count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Discount;");

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void InitializeDatabase_ShouldNotInsertSeedData_WhenNotEmpty()
    {
        // Arrange
        var tempDir = CreateTempFolder();
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", tempDir);
        var factory1 = new ConnectionFactory();
        using (var conn = factory1.Create())
        {
            conn.Execute("INSERT INTO Discount (Name, Type, Priority) VALUES ('Test', 1, 99);");
        }
        var factory2 = new ConnectionFactory();
        using var conn2 = factory2.Create();

        // Act
        var count = conn2.ExecuteScalar<int>("SELECT COUNT(*) FROM Discount;");

        // Assert
        Assert.Equal(3, count);
    }
}
