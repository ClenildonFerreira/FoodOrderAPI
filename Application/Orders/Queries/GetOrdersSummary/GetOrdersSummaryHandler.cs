using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;
using MediatR;

namespace FoodOrderAPI.Application.Orders.Queries.GetOrdersSummary;

public class GetOrdersSummaryHandler : IRequestHandler<GetOrdersSummaryQuery, OrderSummaryDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersSummaryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderSummaryDto> Handle(GetOrdersSummaryQuery query, CancellationToken cancellationToken)
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