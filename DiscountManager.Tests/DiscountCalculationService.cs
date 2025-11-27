using DiscountManager.DiscountCalculation;
using DiscountManager.Discounts;
using NSubstitute;

namespace DiscountManager.Tests;

public class DiscountCalculationServiceTests
{
    private IDiscountRepository MockRepository(params Discount[] discounts)
    {
        var repo = Substitute.For<IDiscountRepository>();

        foreach (var d in discounts)
        {
            repo.Get(d.Name).Returns(Task.FromResult<Discount?>(d));
        }

        return repo;
    }

    [Fact]
    public async Task Calculate_ShouldApplyPercentageDiscount()
    {
        // Arrange
        var discount = new Discount
        {
            Id = 1,
            Name = "Promotion",
            Type = DiscountType.Percentage,
            Priority = 1
        };
        var repository = MockRepository(discount);
        var service = new DiscountCalculationService(repository);
        var available = new Dictionary<string, decimal>
        {
            { "Promotion", 10 }
        };

        // Act
        var result = await service.Calculate(100m, available);

        // Assert
        Assert.Equal(90m, result.AmountAfterDiscount);
        Assert.Single(result.Analysis);
        Assert.Equal(10m, result.Analysis.First().DiscountAmount);
    }

    [Fact]
    public async Task Calculate_ShouldApplyFixedAmountDiscount()
    {
        // Arrange
        var discount = new Discount
        {
            Id = 1,
            Name = "Price List Discount",
            Type = DiscountType.FixedAmount,
            Priority = 1
        };
        var repository = MockRepository(discount);
        var service = new DiscountCalculationService(repository);
        var available = new Dictionary<string, decimal>
        {
            { "Price List Discount", 5 }
        };

        // Act
        var result = await service.Calculate(50m, available);

        // Assert
        Assert.Equal(45m, result.AmountAfterDiscount);
        Assert.Equal(5m, result.Analysis.First().DiscountAmount);
    }

    [Fact]
    public async Task Calculate_ShouldApplyDiscountsByPriority()
    {
        // Arrange
        var priceListDiscount = new Discount { Id = 1, Name = "Price List Discount", Type = DiscountType.Percentage, Priority = 1 };
        var promotionDiscount = new Discount { Id = 2, Name = "Promotion", Type = DiscountType.Percentage, Priority = 2 };
        var couponDiscount = new Discount { Id = 2, Name = "Coupon", Type = DiscountType.FixedAmount, Priority = 3 };
        var repo = MockRepository(promotionDiscount, priceListDiscount, couponDiscount);
        var service = new DiscountCalculationService(repo);
        var available = new Dictionary<string, decimal>
        {
            { "Price List Discount", 5 },
            { "Promotion", 10 },
            { "Coupon", 10 }
        };

        // Act
        var result = await service.Calculate(340m, available);

        // Assert
        Assert.Equal(280.7m, result.AmountAfterDiscount);
        Assert.Equal(new[] { "Price List Discount", "Promotion", "Coupon" }, result.Analysis.Select(a => a.Discount.Name));
        Assert.Equal(17m, result.Analysis.First(a => a.Discount.Name == "Price List Discount").DiscountAmount);
        Assert.Equal(32.3m, result.Analysis.First(a => a.Discount.Name == "Promotion").DiscountAmount);
        Assert.Equal(10m, result.Analysis.First(a => a.Discount.Name == "Coupon").DiscountAmount);
        Assert.Equal(3, result.Analysis.Count());
    }

    [Fact]
    public async Task Calculate_ShouldIgnoreDiscountsNotInRepository()
    {
        // Arrange
        var repository = MockRepository(); // empty repo
        var service = new DiscountCalculationService(repository);

        var available = new Dictionary<string, decimal>
        {
            { "Unknown", 10 }
        };

        // Act
        var result = await service.Calculate(100m, available);

        // Assert
        Assert.Equal(100m, result.AmountAfterDiscount);
        Assert.Empty(result.Analysis);
        await repository.Received(1).Get("Unknown");
    }

    [Fact]
    public async Task GetAvailableDiscounts_ShouldCallRepositoryForEachName()
    {
        // Arrange
        var d1 = new Discount { Name = "A", Type = DiscountType.FixedAmount, Priority = 1 };
        var d2 = new Discount { Name = "B", Type = DiscountType.FixedAmount, Priority = 2 };

        var repo = MockRepository(d1, d2);
        var service = new DiscountCalculationService(repo);

        var available = new Dictionary<string, decimal>
        {
            { "A", 5 },
            { "B", 10 }
        };

        // Act
        await service.Calculate(100m, available);

        // Assert
        await repo.Received(1).Get("A");
        await repo.Received(1).Get("B");
    }

    [Fact]
    public async Task Calculate_ShouldThrowOnUnsupportedDiscountType()
    {
        // Arrange
        var invalid = new Discount
        {
            Id = 1,
            Name = "Invalid",
            Type = (DiscountType)999,  // invalid enum entry
            Priority = 1
        };

        var repository = MockRepository(invalid);
        var service = new DiscountCalculationService(repository);

        var available = new Dictionary<string, decimal>
        {
            { "Invalid", 10 }
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.Calculate(100m, available));
    }
}