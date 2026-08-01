using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderAPI.Application.Products.Queries.GetProducts;

namespace FoodOrderAPI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly GetProductsHandler _getProductsHandler;

    public ProductsController(IProductService productService, GetProductsHandler getProductsHandler)
    {
        _productService = productService;
        _getProductsHandler = getProductsHandler;
    }
    
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var result = await _getProductsHandler.Handle(query);
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