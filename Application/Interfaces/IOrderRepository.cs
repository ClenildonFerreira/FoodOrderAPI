using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<(List<Order> Orders, int TotalCount)> GetPagedAsync(
        OrderStatus? status,
        OrderType? type,
        int page,
        int pageSize);
    Task<Dictionary<OrderStatus, int>> GetSummaryAsync();
    Task AddAsync(Order order);
    Task SaveChangesAsync();
}