using FoodOrderAPI.Application.Services;
using FoodOrderAPI.Domain.Entities;
using FoodOrderAPI.Infrastructure.Data;
using FoodOrderAPI.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Tests.Application.Services;

public class ProductServiceTests
{
    private (ProductService Service, AppDbContext Context) CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        context.Products.AddRange(
            new Product { Id = 1, Name = "Pizza", Price = 45.90m, IsActive = true, Category = "Massas" },
            new Product { Id = 2, Name = "Suco", Price = 12.00m, IsActive = true, Category = "Bebidas" },
            new Product { Id = 3, Name = "Prato Antigo", Price = 20.00m, IsActive = false }
        );
        context.SaveChanges();

        var productRepository = new ProductRepository(context);
        var service = new ProductService(productRepository, new FakeHttpClientFactory());

        return (service, context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveProducts()
    {
        var (service, _) = CreateService();

        var result = await service.GetAllAsync();
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.Name == "Pizza" || p.Name == "Suco");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExistsAndActive()
    {
        var (service, _) = CreateService();

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Pizza");
        result.Price.Should().Be(45.90m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductIsInactive()
    {
        var (service, _) = CreateService();

        var result = await service.GetByIdAsync(3);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ImportFromTheMealDBAsync_ShouldImportProducts()
    {
        var (service, context) = CreateService();

        var imported = await service.ImportFromTheMealDBAsync(2);

        imported.Should().BeGreaterThanOrEqualTo(0);

        var products = await context.Products
            .Where(p => p.ExternalId != null)
            .ToListAsync();

        products.Should().HaveCount(imported);
        products.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var (service, _) = CreateService();

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProduct()
    {
        var (service, context) = CreateService();

        var product = new Product
        {
            Name = "Hambúrguer",
            Price = 32.90m,
            Category = "Lanches",
            IsActive = true
        };

        var result = await service.CreateAsync(product);

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Hambúrguer");

        var fromDb = await context.Products.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
    }
}

file class FakeHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name = "")
    {
        var handler = new FakeHttpMessageHandler();
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.themealdb.com/")
        };
    }
}

file class FakeHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var json = """
        {
          "meals": [
            {
              "idMeal": "52772",
              "strMeal": "Teriyaki Chicken Casserole",
              "strCategory": "Chicken",
              "strInstructions": "Instruções de preparo do prato de teste.",
              "strMealThumb": "https://www.themealdb.com/images/media/meals/wvpsxx1468257224.jpg"
            }
          ]
        }
        """;

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        return Task.FromResult(response);
    }
}
