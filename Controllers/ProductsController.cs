using FoodOrderAPI.DTOs;
using FoodOrderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

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