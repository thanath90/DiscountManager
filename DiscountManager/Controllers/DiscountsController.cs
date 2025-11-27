using DiscountManager.DiscountCalculation;
using DiscountManager.Discounts;
using Microsoft.AspNetCore.Mvc;

namespace DiscountManager.Controllers;

[ApiController]
[Route("[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IDiscountCalculationService _discountCalculationService;

    public DiscountsController(IDiscountRepository discountRepository, IDiscountCalculationService discountCalculationService)
    {
        _discountRepository = discountRepository;
        _discountCalculationService = discountCalculationService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var discounts = await _discountRepository.GetAsync();
        return Ok(discounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var discount = await _discountRepository.Get(id);
        if (discount is null)
        {
            return NotFound();
        }
        return Ok(discount);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Discount discount)
    {
        if (await _discountRepository.PriorityExists(0, discount.Priority))
        {
            return Conflict("A discount with the same priority already exists.");
        }

        var createdDiscount = await _discountRepository.Create(discount);
        return CreatedAtAction(nameof(Get), new { id = createdDiscount.Id }, createdDiscount);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Discount discount)
    {
        if (!await DiscountExists(id))
        {
            return NotFound();
        }

        if (await _discountRepository.PriorityExists(id, discount.Priority))
        {
            return Conflict("A discount with the same priority already exists.");
        }

        discount.Id = id;

        var updatedDiscount = await _discountRepository.Update(discount);
        return Ok(updatedDiscount);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await DiscountExists(id))
        {
            return NotFound();
        }
        await _discountRepository.Delete(id);
        return NoContent();
    }

    private async Task<bool> DiscountExists(int id)
    {
        var discount = await _discountRepository.Get(id);
        return discount is not null;
    }
}