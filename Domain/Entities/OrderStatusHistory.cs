using FoodOrderAPI.Domain.Common;

namespace FoodOrderAPI.Domain.Entities;

public class OrderStatusHistory : Entity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}