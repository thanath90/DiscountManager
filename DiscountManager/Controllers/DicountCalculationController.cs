using DiscountManager.DiscountCalculation;
using Microsoft.AspNetCore.Mvc;

namespace DiscountManager.Controllers;

[ApiController]
[Route("discount-calculation")]
public class DicountCalculationController : ControllerBase
{
    private readonly IDiscountCalculationService _discountCalculationService;

    public DicountCalculationController(IDiscountCalculationService discountCalculationService)
    {
        _discountCalculationService = discountCalculationService;
    }

    [HttpPost]
    public async Task<IActionResult> CalculateDiscounts([FromBody] DiscountCalculationRequest request)
    {
        var result = await _discountCalculationService.Calculate(request.OrderAmount, request.AvailableDiscounts);
        return Ok(result);
    }
}
