using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _productService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportFromTheMealDB([FromQuery] int quantity = 10)
    {
        var imported = await _productService.ImportFromTheMealDBAsync(quantity);

        return Ok(new
        {
            message = $"{imported} produto(s) importado(s) com sucesso do TheMealDB.",
            requested = quantity,
            imported
        });
    }
}