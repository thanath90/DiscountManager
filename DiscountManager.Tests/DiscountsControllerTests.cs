using DiscountManager.Controllers;
using DiscountManager.DiscountCalculation;
using DiscountManager.Discounts;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Xml.Linq;

namespace DiscountManager.Tests;

public class DiscountsControllerTests
{
    private readonly IDiscountRepository _repo;
    private readonly IDiscountCalculationService _calcService;
    private readonly DiscountsController _controller;

    public DiscountsControllerTests()
    {
        _repo = Substitute.For<IDiscountRepository>();
        _calcService = Substitute.For<IDiscountCalculationService>();
        _controller = new DiscountsController(_repo, _calcService);
    }

    [Fact]
    public async Task Get_ReturnsOk_WithDiscounts()
    {
        // Arrange
        var discounts = new List<Discount>
        {
            new() { Id = 1, Name = "Promotion", Priority = 1, Type = DiscountType.FixedAmount }
        };

        _repo.GetAsync().Returns(discounts);

        // Act
        var result = await _controller.Get() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(discounts, result.Value);
    }

    [Fact]
    public async Task Get_ById_ReturnsOk_WhenFound()
    {
        // Arrange
        var discount = new Discount { Id = 1, Name = "A", Priority = 1, Type = DiscountType.FixedAmount };
        _repo.Get(1).Returns(discount);

        // Act
        var result = await _controller.Get(1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(discount, result.Value);
    }

    [Fact]
    public async Task Get_ById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _repo.Get(1).Returns((Discount?)null);

        // Act
        var result = await _controller.Get(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenPriorityExists()
    {
        // Arrange
        var discount = new Discount { Name = "Test", Priority = 2 };
        _repo.PriorityExists(0, discount.Priority).Returns(true);

        // Act
        var result = await _controller.Create(discount);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenSuccessful()
    {
        // Arrange
        var discount = new Discount { Id = 1, Name = "Test", Priority = 1 };
        _repo.PriorityExists(0, discount.Priority).Returns(false);
        _repo.Create(discount).Returns(discount);

        // Act
        var result = await _controller.Create(discount) as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Get", result.ActionName);
        Assert.Equal(discount, result.Value);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenDiscountDoesNotExist()
    {
        // Arrange
        _repo.Get(Arg.Any<int>()).Returns((Discount?)null);

        // Act
        var result = await _controller.Update(1, new Discount { Name = "Price List Discount", Type = DiscountType.Percentage, Priority = 1 });

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsConflict_WhenPriorityExists()
    {
        // Arrange
        var discount = new Discount { Id = 1, Name = "Price List Discount", Type = DiscountType.Percentage, Priority = 1 };
        _repo.Get(1).Returns(discount);
        _repo.PriorityExists(1, discount.Priority).Returns(true);

        // Act
        var result = await _controller.Update(1, discount);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var discount = new Discount { Id = 1, Name = "Price List Discount", Type = DiscountType.Percentage, Priority = 1 };
        var updated = new Discount { Id = 1, Name = "Promotion", Type = DiscountType.Percentage, Priority = 2 };
        _repo.Get(1).Returns(discount);
        _repo.PriorityExists(1, Arg.Any<int>()).Returns(false);
        _repo.Update(Arg.Any<Discount>()).Returns(updated);

        // Act
        var result = await _controller.Update(1, discount) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updated, result.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenDiscountDoesNotExist()
    {
        // Arrange
        _repo.Get(1).Returns((Discount?)null);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var discount = new Discount { Id = 1, Name = "Price List Discount", Type = DiscountType.Percentage, Priority = 1 };
        _repo.Get(1).Returns(discount);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        await _repo.Received(1).Delete(1);
        Assert.IsType<NoContentResult>(result);
    }
}