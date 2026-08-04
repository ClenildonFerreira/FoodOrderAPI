using System.Diagnostics;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodOrderAPI.Application.Products.Commands.ImportProducts;

public class ImportProductsHandler : IRequestHandler<ImportProductsCommand, ImportProductsResult>
{
    private readonly IProductRepository _productRepository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImportProductsHandler> _logger;

    public ImportProductsHandler(
        IProductRepository productRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<ImportProductsHandler> logger)
    {
        _productRepository = productRepository;
        _httpClient = httpClientFactory.CreateClient("TheMealDB");
        _logger = logger;
    }

    public async Task<ImportProductsResult> Handle(ImportProductsCommand command, CancellationToken cancellationToken)
    {
        var totalSw = Stopwatch.StartNew();

        var quantity = Math.Clamp(command.Quantity, 0, 50); // reduced max to protect TheMealDB and our performance
        if (quantity == 0)
        {
            _logger.LogInformation("Import skipped: quantity is zero");
            return new ImportProductsResult(0, 0, 0, 0);
        }

        _logger.LogInformation("Starting product import. Requested quantity: {Quantity}", quantity);

        // 1. Fetch from TheMealDB
        var fetchSw = Stopwatch.StartNew();
        var (responses, failedHttp) = await FetchMealsAsync(quantity, cancellationToken);
        fetchSw.Stop();

        _logger.LogInformation(
            "TheMealDB fetch completed in {ElapsedMs}ms. Successful: {SuccessCount}, Failed HTTP: {FailedCount}",
            fetchSw.ElapsedMilliseconds,
            responses.Count,
            failedHttp);

        // 2. Load existing IDs (still full load - can be optimized later)
        var existingSw = Stopwatch.StartNew();
        var existingIds = await _productRepository.GetExistingExternalIdsAsync();
        existingSw.Stop();

        _logger.LogDebug("Loaded {Count} existing ExternalIds in {ElapsedMs}ms", existingIds.Count, existingSw.ElapsedMilliseconds);

        // 3. Parse and filter
        var productsToAdd = new List<Product>();
        var skipped = 0;

        foreach (var response in responses)
        {
            var product = ParseMeal(response, existingIds);
            if (product is null)
            {
                skipped++;
                continue;
            }

            productsToAdd.Add(product);
            existingIds.Add(product.ExternalId!);
        }

        // 4. Persist
        if (productsToAdd.Count > 0)
        {
            var saveSw = Stopwatch.StartNew();
            await _productRepository.AddRangeAsync(productsToAdd);
            await _productRepository.SaveChangesAsync();
            saveSw.Stop();

            _logger.LogInformation("Persisted {Count} products in {ElapsedMs}ms", productsToAdd.Count, saveSw.ElapsedMilliseconds);
        }

        totalSw.Stop();

        var result = new ImportProductsResult(
            Imported: productsToAdd.Count,
            Skipped: skipped,
            FailedHttp: failedHttp,
            DurationMs: totalSw.ElapsedMilliseconds);

        _logger.LogInformation(
            "Import finished. Imported: {Imported}, Skipped: {Skipped}, FailedHttp: {FailedHttp}, TotalDuration: {DurationMs}ms",
            result.Imported,
            result.Skipped,
            result.FailedHttp,
            result.DurationMs);

        return result;
    }

    private async Task<(List<string> Responses, int FailedCount)> FetchMealsAsync(int quantity, CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(5);

        var tasks = Enumerable.Range(0, quantity).Select(async _ =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                // Relative path because BaseAddress is configured
                return await _httpClient.GetStringAsync("random.php", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch meal from TheMealDB");
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        var successful = results.Where(r => r is not null).Cast<string>().ToList();
        var failed = results.Count(r => r is null);

        return (successful, failed);
    }

    private static Product? ParseMeal(string response, HashSet<string> existingIds)
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
