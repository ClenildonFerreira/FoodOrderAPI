using FoodOrderAPI.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderAPI.Application.Products.Queries.GetProducts;
using FoodOrderAPI.Application.Products.Queries.GetProductById;
using FoodOrderAPI.Application.Products.Commands.ImportProducts;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;

namespace FoodOrderAPI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("FixedWindowPolicy")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
        
    }
    
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        if (product is null) return NotFound();
        return Ok(product);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromQuery] int quantity = 10)
    {
        var command = new ImportProductsCommand { Quantity = quantity };
        var importedCount = await _mediator.Send(command);

        return Ok(new { message = $"{importedCount} produtos importados com sucesso." });
    }
}