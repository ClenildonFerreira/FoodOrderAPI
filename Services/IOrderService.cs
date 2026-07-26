using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;


namespace FoodOrderAPI.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<PagedResultDto<OrderDto>> GetAllAsync(OrderStatus? status = null, int page = 1, int pageSize = 10);
        Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
    }
}