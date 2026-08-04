using FoodOrderAPI.Domain.Common;
using FoodOrderAPI.Domain.Services;
using FoodOrderAPI.Domain.ValueObjects;

namespace FoodOrderAPI.Domain.Entities;

public class Order : AggregateRoot
{
    public string CustomerName { get; set; } = string.Empty;
    public string? TableNumber { get; set; }
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Received;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Money Total { get;  set; } = Money.Zero();

    public List<OrderItem> Items { get; set; } = new();
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();

    private Order() { }

    public static Result<Order> Create(
        string customerName,
        string? tableNumber,
        OrderType type,
        List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return Result.Failure<Order>("Nome do cliente é obrigatório.");

        if (items is null || !items.Any())
            return Result.Failure<Order>("O pedido deve conter pelo menos 1 item.");

        if (items.Any(i => i.Quantity <= 0))
            return Result.Failure<Order>("A quantidade de cada item deve ser maior que zero.");

        if (type == OrderType.Table && string.IsNullOrWhiteSpace(tableNumber))
            return Result.Failure<Order>("Número da mesa é obrigatório para pedidos de salão.");

        var order = new Order
        {
            CustomerName = customerName.Trim(),
            TableNumber = tableNumber?.Trim(),
            Type = type,
            Status = OrderStatus.Received,
            CreatedAt = DateTime.UtcNow,
            Items = items,
            Total =  new Money(items.Sum(i => i.UnitPrice.Amount * i.Quantity))
        };

        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Received,
            Notes = "Pedido criado"
        });

        return Result.Success(order);
    }

    public Result ChangeStatus(OrderStatus newStatus, string? notes = null)
    {
        if (!OrderStatusTransition.CanTransition(Status, newStatus))
        {
            return Result.Failure(
                OrderStatusTransition.GetErrorMessage(Status, newStatus));
        }

        Status = newStatus;
        StatusHistory.Add(new OrderStatusHistory
        {
            Status = newStatus,
            Notes = notes ?? $"Status alterado para {newStatus}"
        });

        return Result.Success();
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