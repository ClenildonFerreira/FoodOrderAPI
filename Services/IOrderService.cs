using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<List<OrderDto>> GetAllAsync();
        Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
    }
}