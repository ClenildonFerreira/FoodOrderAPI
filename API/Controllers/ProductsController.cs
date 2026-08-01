using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderAPI.Application.Products.Queries.GetProducts;
using FoodOrderAPI.Application.Products.Queries.GetProductById;
using FoodOrderAPI.Application.Products.Commands.ImportProducts;

namespace FoodOrderAPI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly GetProductsHandler _getProductsHandler;
    private readonly GetProductByIdHandler _getProductByIdHandler;
    private readonly ImportProductsHandler _importProductsHandler;

    public ProductsController(ImportProductsHandler importProductsHandler, 
                                GetProductsHandler getProductsHandler, 
                                GetProductByIdHandler getProductByIdHandler
                            )
    {
        _getProductsHandler = getProductsHandler;
        _getProductByIdHandler = getProductByIdHandler;
        _importProductsHandler = importProductsHandler;
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
        var product = await _getProductByIdHandler.Handle(new GetProductByIdQuery(id));
        if (product is null) return NotFound();
        return Ok(product);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromQuery] int quantity = 10)
    {
        var command = new ImportProductsCommand { Quantity = quantity };
        var importedCount = await _importProductsHandler.Handle(command);

        return Ok(new { message = $"{importedCount} produtos importados com sucesso." });
    }
}