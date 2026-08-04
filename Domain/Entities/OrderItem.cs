using FoodOrderAPI.Domain.Common;
using FoodOrderAPI.Domain.ValueObjects;

namespace FoodOrderAPI.Domain.Entities;
    
public class OrderItem : Entity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public Money UnitPrice { get; set; } = Money.Zero();
}