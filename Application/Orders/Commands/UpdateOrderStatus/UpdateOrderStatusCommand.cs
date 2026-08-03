using MediatR;
using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Domain.Common;


namespace FoodOrderAPI.Application.Orders.Commands.UpdateOrderStatus;


public class UpdateOrderStatusCommand : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; set; }
    public UpdateOrderStatusDto Dto { get; set; } = null!;
}