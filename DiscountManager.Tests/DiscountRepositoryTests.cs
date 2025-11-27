using Dapper;
using DiscountManager.Discounts;
using DiscountManager.Persistence;
using Microsoft.Data.Sqlite;
using NSubstitute;

namespace DiscountManager.Tests;

public class DiscountRepositoryTests
{
    private IDiscountRepository BuildRepository()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);

        var dbPath = Path.Combine(folder, "testdb.sqlite");
        var connString = $"Data Source={dbPath}";

        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            conn.Execute(@"
                CREATE TABLE Discount (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Type INTEGER NOT NULL,
                    Priority INTEGER NOT NULL
                );
            ");
        }

        var factory = Substitute.For<IConnectionFactory>();
        factory.Create().Returns(call =>
        {
            var c = new SqliteConnection(connString);
            c.Open();
            return c;
        });

        return new DiscountRepository(factory);
    }

    [Fact]
    public async Task Create_ShouldInsert_WhenNotExists()
    {
        // Arrange
        var repopository = BuildRepository();

        var discount = new Discount
        {
            Name = "Test",
            Type = 0,
            Priority = 1
        };

        // Act
        var created = await repopository.Create(discount);

        // Assert
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task Create_ShouldReturnExisting_WhenNameExists()
    {
        // Arrange
        var repopository = BuildRepository();
        var discount1 = await repopository.Create(new Discount { Name = "Promo", Type = 0, Priority = 1 });

        // Act
        var discount2 = await repopository.Create(new Discount { Name = "Promo", Type = 0, Priority = 2 });

        // Assert
        Assert.Equal(discount1.Id, discount2.Id);
    }

    [Fact]
    public async Task Get_ById_ShouldReturnCorrectDiscount()
    {
        // Arrange
        var repopository = BuildRepository();

        var created = await repopository.Create(new Discount
        {
            Name = "Test",
            Type = DiscountType.FixedAmount,
            Priority = 9
        });

        // Act
        var loaded = await repopository.Get(created.Id!.Value);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Test", loaded!.Name);
        Assert.Equal(DiscountType.FixedAmount, loaded.Type);
        Assert.Equal(9, loaded.Priority);
    }

    [Fact]
    public async Task Get_ByName_ShouldReturnCorrectDiscount()
    {
        // Arrange
        var repository = BuildRepository();
        await repository.Create(new Discount { Name = "Unique", Type = DiscountType.FixedAmount, Priority = 3 });

        // Act
        var loaded = await repository.Get("Unique");

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Unique", loaded!.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllRecords()
    {
        // Arrange
        var repopository = BuildRepository();
        await repopository.Create(new Discount { Name = "A", Type = 0, Priority = 1 });
        await repopository.Create(new Discount { Name = "B", Type = 0, Priority = 2 });

        // Act
        var all = await repopository.GetAsync();

        // Assert
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task PriorityExists_ShouldReturnTrue_WhenAnotherHasSamePriority()
    {
        // Arrange
        var repopository = BuildRepository();
        var discount1 = await repopository.Create(new Discount { Name = "X", Type = 0, Priority = 5 });
        var discount2 = await repopository.Create(new Discount { Name = "Y", Type = 0, Priority = 7 });

        // Act
        var exists = await repopository.PriorityExists(discount2.Id!.Value, 5);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task PriorityExists_ShouldReturnFalse_WhenNoOtherHasSamePriority()
    {
        // Arrange
        var repopository = BuildRepository();
        var discount = await repopository.Create(new Discount { Name = "X", Type = 0, Priority = 5 });

        // Act
        var exists = await repopository.PriorityExists(discount.Id!.Value, 99);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task Update_ShouldModifyRecord()
    {
        // Arrange
        var repopository = BuildRepository();
        var created = await repopository.Create(new Discount
        {
            Name = "Original",
            Type = DiscountType.FixedAmount,
            Priority = 1
        });
        created.Name = "Updated";
        created.Type = DiscountType.Percentage;
        created.Priority = 100;

        // Act
        var updated = await repopository.Update(created);

        // Assert
        Assert.Equal("Updated", updated.Name);
        Assert.Equal(DiscountType.Percentage, updated.Type);
        Assert.Equal(100, updated.Priority);
    }

    [Fact]
    public async Task Delete_ShouldRemoveRecord()
    {
        // Arrange
        var repopository = BuildRepository();
        var created = await repopository.Create(new Discount
        {
            Name = "ToDelete",
            Type = 0,
            Priority = 1
        });
        
        // Act
        await repopository.Delete(created.Id!.Value);

        //Assert        
        var loaded = await repopository.Get(created.Id!.Value);
        Assert.Null(loaded);
    }
}
