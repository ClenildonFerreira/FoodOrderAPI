using FoodOrderAPI.API.Extensions;
using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Orders.Commands.CreateOrder;
using FoodOrderAPI.Application.Orders.Commands.UpdateOrderStatus;
using FoodOrderAPI.Application.Orders.Queries.GetOrderById;
using FoodOrderAPI.Application.Orders.Queries.GetOrders;
using FoodOrderAPI.Application.Orders.Queries.GetOrdersSummary;
using FoodOrderAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace FoodOrderAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetAll(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] OrderType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetOrdersQuery
        {
            Status = status,
            Type = type,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<OrderSummaryDto>> GetSummary()
    {
        var summary = await _mediator.Send(new GetOrdersSummaryQuery());
        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToCreatedAtActionResult(this, nameof(GetById), order => new { id = order.Id });
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
    {
        command.OrderId = id;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
