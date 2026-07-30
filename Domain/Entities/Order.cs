using FoodOrderAPI.Domain.Services;

namespace FoodOrderAPI.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? TableNumber { get; private set; }
    public OrderType Type { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Received;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public decimal Total { get; private set; }    

    public List<OrderItem> Items { get; private set; } = new();
    public List<OrderStatusHistory> StatusHistory { get; private set; } = new();

    public Order() { }

    public Order(string customerName, string? tableNumber, OrderType type, List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Nome do cliente é obrigatório.");

        if (items is null || !items.Any())
            throw new ArgumentException("O pedido deve conter pelo menos 1 item.");

        if (items.Any(i => i.Quantity <= 0))
            throw new ArgumentException("A quantidade de cada item deve ser maior que zero.");

        if (type == OrderType.Table && string.IsNullOrWhiteSpace(tableNumber))
            throw new ArgumentException("Número da mesa é obrigatório para pedidos de salão.");

        CustomerName = customerName.Trim();
        TableNumber = tableNumber?.Trim();
        Type = type;
        Status = OrderStatus.Received;
        CreatedAt = DateTime.UtcNow;
        Items = items;
        Total = items.Sum(i => i.UnitPrice * i.Quantity);

        StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Received,
            Notes = "Pedido criado"
        });
    }

    public void ChangeStatus(OrderStatus newStatus, string? notes = null)
    {
        if (!OrderStatusTransition.CanTransition(Status, newStatus))
        {
            throw new InvalidOperationException(
                OrderStatusTransition.GetErrorMessage(Status, newStatus)
            );
        }

        Status = newStatus;
        StatusHistory.Add(new OrderStatusHistory
        {
            Status = newStatus,
            Notes = notes ?? $"Status alterado para {newStatus}"
        });
    }
}

    public enum OrderType
    {
        Table = 1,
        Delivery = 2,
    }

    public enum OrderStatus
    {
        Received = 1, 
        Preparing = 2,
        Ready = 3,
        Delivered = 4,
        Cancelled = 5,
    }