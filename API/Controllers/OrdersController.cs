using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Commands.CreateOrder;
using FoodOrderAPI.Application.Orders.Commands.UpdateOrderStatus;
using FoodOrderAPI.Application.Orders.Queries.GetOrderById;
using FoodOrderAPI.Application.Orders.Queries.GetOrders;
using FoodOrderAPI.Application.Orders.Queries.GetOrdersSummary;
using FoodOrderAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
    private readonly GetOrderByIdHandler _getOrderByIdHandler;
    private readonly GetOrdersHandler _getOrdersHandler;
    private readonly GetOrdersSummaryHandler _getOrdersSummaryHandler;



    public OrdersController(
            CreateOrderHandler createOrderHandler,
            UpdateOrderStatusHandler updateOrderStatusHandler,
            GetOrderByIdHandler getOrderByIdHandler,
            GetOrdersHandler getOrdersHandler,
            GetOrdersSummaryHandler getOrdersSummaryHandler
            )
    {
        _createOrderHandler = createOrderHandler;
        _updateOrderStatusHandler = updateOrderStatusHandler;
        _getOrderByIdHandler = getOrderByIdHandler;
        _getOrdersHandler = getOrdersHandler;
        _getOrdersSummaryHandler = getOrdersSummaryHandler;
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

        var result = await _getOrdersHandler.Handle(query);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<OrderSummaryDto>> GetSummary()
    {
        var summary = await _getOrdersSummaryHandler.Handle(new GetOrdersSummaryQuery());
        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _getOrderByIdHandler.Handle(new GetOrderByIdQuery(id));
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
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusCommand command)
    {
        command.OrderId = id;
        var order = await _updateOrderStatusHandler.Handle(command);
        if (order is null) return NotFound();
        return Ok(order);
    }
}