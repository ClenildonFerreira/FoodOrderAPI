using FoodOrderAPI.Data;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FoodOrderAPI.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public ProductService(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = _context.Products.Where(p => p.IsActive);

        var totalItems = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Category = p.Category
            })
            .ToListAsync();

        return new PagedResultDto<ProductDto>
        {
            Items = products,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null || !product.IsActive) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            Category = product.Category
        };
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<int> ImportFromTheMealDBAsync(int quantity = 10)
    {
        if (quantity <= 0) return 0;
        if (quantity > 100) quantity = 100;

        var importedCount = 0;
        using var semaphore = new SemaphoreSlim(5); 

        var tasks = Enumerable.Range(0, quantity).Select(async _ =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await _httpClient.GetStringAsync("https://www.themealdb.com/api/json/v1/1/random.php");
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        var responses = await Task.WhenAll(tasks);
        var existingExternalIds = await _context.Products
            .Where(p => p.ExternalId != null)
            .Select(p => p.ExternalId!)
            .ToHashSetAsync();

        foreach (var response in responses)
        {
            if (string.IsNullOrWhiteSpace(response))
                continue;

            try
            {
                using var doc = JsonDocument.Parse(response);

                if (!doc.RootElement.TryGetProperty("meals", out var meals) ||
                    meals.GetArrayLength() == 0)
                    continue;

                var meal = meals[0];
                var externalId = meal.GetProperty("idMeal").GetString();

                if (string.IsNullOrWhiteSpace(externalId))
                    continue;

                if (existingExternalIds.Contains(externalId))
                    continue;

                var instructions = meal.GetProperty("strInstructions").GetString() ?? "";
                var description = instructions.Length > 300
                    ? instructions[..300]
                    : instructions;

                var product = new Product
                {
                    Name = meal.GetProperty("strMeal").GetString() ?? "Sem nome",
                    Description = description,
                    Category = meal.GetProperty("strCategory").GetString(),
                    ImageUrl = meal.GetProperty("strMealThumb").GetString(),
                    ExternalId = externalId,
                    Price = Random.Shared.Next(25, 90) + 0.90m,
                    IsActive = true
                };

                _context.Products.Add(product);
                existingExternalIds.Add(externalId); // Registra no hashset temporário da execução
                importedCount++;
            }
            catch (JsonException)
            {
                continue;
            }
            catch (Exception)
            {
                continue;
            }
        }

        if (importedCount > 0)
            await _context.SaveChangesAsync();

        return importedCount;
    }
}