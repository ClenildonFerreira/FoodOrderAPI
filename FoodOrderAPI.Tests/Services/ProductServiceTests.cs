using FoodOrderAPI.Data;
using FoodOrderAPI.Models;
using FoodOrderAPI.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Tests.Services;

public class ProductServiceTests
{
    private AppDbContext CreateInMemoryContext()
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

        return context;
    }

    private ProductService CreateService(AppDbContext context)
    {
        var httpClientFactory = new FakeHttpClientFactory();
        return new ProductService(context, httpClientFactory);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveProducts()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Name == "Pizza" || p.Name == "Suco");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExistsAndActive()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Pizza");
        result.Price.Should().Be(45.90m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductIsInactive()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(3);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProduct()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

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

// Fake simples para não precisar de HTTP real nos testes
public class FakeHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name = "")
    {
        return new HttpClient();
    }
}