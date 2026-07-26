using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;


namespace FoodOrderAPI.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<List<OrderDto>> GetAllAsync(OrderStatus? status = null);
        Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
    }
}