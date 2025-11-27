using DiscountManager.Controllers;
using DiscountManager.DiscountCalculation;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace DiscountManager.Tests;

public class DiscountCalculationControllerTests
{
    private readonly IDiscountCalculationService _discountCalculationService;
    private readonly DicountCalculationController _controller;

    public DiscountCalculationControllerTests()
    {
        _discountCalculationService = Substitute.For<IDiscountCalculationService>();
        _controller = new DicountCalculationController(_discountCalculationService);
    }

    [Fact]
    public async Task CalculateDiscounts_ReturnsOk_WithCorrectResult()
    {
        // Arrange
        var request = new DiscountCalculationRequest(100, new() { { "Promo", 10 } });
        var expectedOrder = new DiscountOrder(100, 90, []);
        _discountCalculationService
            .Calculate(request.OrderAmount, request.AvailableDiscounts)
            .Returns(expectedOrder);

        // Act
        var result = await _controller.CalculateDiscounts(request) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(expectedOrder, result.Value);
    }
}
