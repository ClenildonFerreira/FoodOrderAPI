using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Orders.Queries.GetOrders;

public class GetOrdersHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResultDto<OrderDto>> Handle(GetOrdersQuery query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 50) query.PageSize = 50;

        var (orders, totalItems) = await _orderRepository.GetPagedAsync(
            query.Status,
            query.Type,
            query.Page,
            query.PageSize
        );

        return new PagedResultDto<OrderDto>
        {
            Items = orders.Select(MapToDto).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems
        };
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