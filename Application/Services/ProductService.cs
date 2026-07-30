using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;
using System.Text.Json;

namespace FoodOrderAPI.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly HttpClient _httpClient;

    public ProductService(IProductRepository productRepository, IHttpClientFactory httpClientFactory)
    {
        _productRepository = productRepository;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var (products, totalItems) = await _productRepository.GetActivePagedAsync(page, pageSize);

        return new PagedResultDto<ProductDto>
        {
            Items = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Category = p.Category
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
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
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();
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
            catch
            {
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        var responses = await Task.WhenAll(tasks);
        var existingExternalIds = await _productRepository.GetExistingExternalIdsAsync();
        var productsToAdd = new List<Product>();

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

                if (string.IsNullOrWhiteSpace(externalId) || existingExternalIds.Contains(externalId))
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

                productsToAdd.Add(product);
                existingExternalIds.Add(externalId);
                importedCount++;
            }
            catch
            {
                continue;
            }
        }

        if (productsToAdd.Any())
        {
            await _productRepository.AddRangeAsync(productsToAdd);
            await _productRepository.SaveChangesAsync();
        }

        return importedCount;
    }
}