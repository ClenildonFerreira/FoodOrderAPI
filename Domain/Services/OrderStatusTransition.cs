using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Domain.Services;

public static class OrderStatusTransition
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedTransitions = new()
    {
        { OrderStatus.Received, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.Cancelled } },
        { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.Ready, OrderStatus.Cancelled } },
        { OrderStatus.Ready, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Cancelled } },
        { OrderStatus.Delivered, new List<OrderStatus>() },
        { OrderStatus.Cancelled, new List<OrderStatus>() }
    };

    public static bool CanTransition(OrderStatus current, OrderStatus next)
    {
        return AllowedTransitions.ContainsKey(current)
            && AllowedTransitions[current].Contains(next);
    }

    public static string GetErrorMessage(OrderStatus current, OrderStatus next)
    {
        return $"Não é permitido alterar o status de '{current}' para '{next}'.";
    }
}