using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;


namespace FoodOrderAPI.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<PagedResultDto<OrderDto>> GetAllAsync(
            OrderStatus? status = null,
            OrderType? type = null,
            int page = 1,
            int pageSize = 10);
        Task<OrderSummaryDto> GetSummaryAsync();
        Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
    }
}