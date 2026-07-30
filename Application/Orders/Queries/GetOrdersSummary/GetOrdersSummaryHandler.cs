using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Orders.Queries.GetOrdersSummary;

public class GetOrdersSummaryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersSummaryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderSummaryDto> Handle(GetOrdersSummaryQuery query)
    {
        var summary = await _orderRepository.GetSummaryAsync();

        return new OrderSummaryDto
        {
            Received = summary.GetValueOrDefault(OrderStatus.Received),
            Preparing = summary.GetValueOrDefault(OrderStatus.Preparing),
            Ready = summary.GetValueOrDefault(OrderStatus.Ready),
            Delivered = summary.GetValueOrDefault(OrderStatus.Delivered),
            Cancelled = summary.GetValueOrDefault(OrderStatus.Cancelled)
        };
    }
}