using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<(List<Product> Products, int TotalCount)> GetActivePagedAsync(int page, int pageSize);
    Task<HashSet<string>> GetExistingExternalIdsAsync();
    Task AddAsync(Product product);
    Task AddRangeAsync(IEnumerable<Product> products);
    Task SaveChangesAsync();
}
