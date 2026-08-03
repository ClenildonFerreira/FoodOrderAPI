using MediatR;
using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Domain.Common;


namespace FoodOrderAPI.Application.Orders.Commands.UpdateOrderStatus;


public class UpdateOrderStatusCommand : IRequest<Result<OrderDto>>
{
    public int OrderId { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
}