using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(UpdateOrderStatusCommand command)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(command.OrderId);
        if (order is null) return null;

        var newStatus = (OrderStatus)command.Status;
        order.ChangeStatus(newStatus, command.Notes);

        await _orderRepository.SaveChangesAsync();

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            TableNumber = order.TableNumber,
            Type = order.Type.ToString(),
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            Total = order.Total,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            StatusHistory = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDto
                {
                    Status = h.Status.ToString(),
                    ChangedAt = h.ChangedAt,
                    Notes = h.Notes
                }).ToList()
        };
    }
}