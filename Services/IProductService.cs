using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;

namespace FoodOrderAPI.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task ImportFromTheMealDBAsync(int quantity = 10);
    }
}