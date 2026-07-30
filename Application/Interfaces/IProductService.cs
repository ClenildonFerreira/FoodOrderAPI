using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResultDto<ProductDto>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<int> ImportFromTheMealDBAsync(int quantity = 10);
    }
}