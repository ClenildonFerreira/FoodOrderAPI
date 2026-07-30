using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;
using FoodOrderAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Infrastructure.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<(List<Product> Products, int TotalCount)> GetActivePagedAsync(int page, int pageSize)
    {
        var query = _context.Products.Where(p => p.IsActive);

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<HashSet<string>> GetExistingExternalIdsAsync()
    {
        return await _context.Products
            .Where(p => p.ExternalId != null)
            .Select(p => p.ExternalId!)
            .ToHashSetAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public async Task AddRangeAsync(IEnumerable<Product> products)
    {
        await _context.Products.AddRangeAsync(products);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}