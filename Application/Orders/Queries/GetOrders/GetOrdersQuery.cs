using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Orders.Queries.GetOrders;

public class GetOrdersQuery
{
    public OrderStatus? Status { get; set; }
    public OrderType? Type { get; set; }
    public int Page { get; set; } 
    public int PageSize { get; set; }
    
}