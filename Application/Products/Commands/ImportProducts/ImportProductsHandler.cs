using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;
using System.Text.Json;

namespace FoodOrderAPI.Application.Products.Commands.ImportProducts;

public class ImportProductsHandler
{
    private readonly IProductRepository _productRepository;
    private readonly HttpClient _httpClient;

    public ImportProductsHandler(
        IProductRepository productRepository,
        IHttpClientFactory httpClientFactory)
    {
        _productRepository = productRepository;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<int> Handle(ImportProductsCommand command)
    {
        var quantity = Math.Clamp(command.Quantity, 0, 100);
        if (quantity == 0) return 0;

        var responses = await FetchMealsAsync(quantity);
        var existingIds = await _productRepository.GetExistingExternalIdsAsync();
        var productsToAdd = new List<Product>();

        foreach (var response in responses)
        {
            var product = ParseMeal(response, existingIds);
            if (product is null) continue;

            productsToAdd.Add(product);
            existingIds.Add(product.ExternalId!);
        }

        if (productsToAdd.Count == 0) return 0;

        await _productRepository.AddRangeAsync(productsToAdd);
        await _productRepository.SaveChangesAsync();

        return productsToAdd.Count;
    }

    private async Task<List<string?>> FetchMealsAsync(int quantity)
    {
        using var semaphore = new SemaphoreSlim(5);

        var tasks = Enumerable.Range(0, quantity).Select(async _ =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await _httpClient.GetStringAsync(
                    "https://www.themealdb.com/api/json/v1/1/random.php");
            }
            catch
            {
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        });

        return (await Task.WhenAll(tasks)).ToList();
    }

    private static Product? ParseMeal(string? response, HashSet<string> existingIds)
    {
        if (string.IsNullOrWhiteSpace(response))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(response);

            if (!doc.RootElement.TryGetProperty("meals", out var meals) ||
                meals.GetArrayLength() == 0)
                return null;

            var meal = meals[0];
            var externalId = meal.GetProperty("idMeal").GetString();

            if (string.IsNullOrWhiteSpace(externalId) || existingIds.Contains(externalId))
                return null;

            var instructions = meal.GetProperty("strInstructions").GetString() ?? "";
            var description = instructions.Length > 300
                ? instructions[..300]
                : instructions;

            return new Product
            {
                Name = meal.GetProperty("strMeal").GetString() ?? "Sem nome",
                Description = description,
                Category = meal.GetProperty("strCategory").GetString(),
                ImageUrl = meal.GetProperty("strMealThumb").GetString(),
                ExternalId = externalId,
                Price = Random.Shared.Next(25, 90) + 0.90m,
                IsActive = true
            };
        }
        catch
        {
            return null;
        }
    }
}