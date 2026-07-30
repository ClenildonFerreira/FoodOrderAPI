using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Commands.CreateOrder;
using FoodOrderAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly CreateOrderHandler _createOrderHandler;


    public OrdersController(IOrderService orderService, CreateOrderHandler createOrderHandler)
    {
        _orderService = orderService;
        _createOrderHandler = createOrderHandler;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetAll(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] OrderType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetAllAsync(status, type, page, pageSize);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<OrderSummaryDto>> GetSummary()
    {
        var summary = await _orderService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderCommand command)
    {
        var order = await _createOrderHandler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _orderService.UpdateStatusAsync(id, dto);
        if (order is null) return NotFound();
        return Ok(order);
    }
}