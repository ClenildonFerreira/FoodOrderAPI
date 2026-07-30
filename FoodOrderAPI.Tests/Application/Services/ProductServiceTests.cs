using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Services;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly ProductService _sut;

    private static readonly Product ActivePizza = new()
    {
        Id = 1,
        Name = "Pizza",
        Price = 45.90m,
        IsActive = true,
        Category = "Massas"
    };

    private static readonly Product ActiveJuice = new()
    {
        Id = 2,
        Name = "Suco",
        Price = 12.00m,
        IsActive = true,
        Category = "Bebidas"
    };

    private static readonly Product InactiveProduct = new()
    {
        Id = 3,
        Name = "Prato Antigo",
        Price = 20.00m,
        IsActive = false
    };

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(new FakeHttpMessageHandler())
            {
                BaseAddress = new Uri("https://www.themealdb.com/")
            });

        _sut = new ProductService(
            _productRepositoryMock.Object,
            _httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveProducts()
    {
        _productRepositoryMock
            .Setup(r => r.GetActivePagedAsync(1, 10))
            .ReturnsAsync((new List<Product> { ActivePizza, ActiveJuice }, 2));

        var result = await _sut.GetAllAsync();

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.Name == "Pizza" || p.Name == "Suco");
        result.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExistsAndActive()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(ActivePizza);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Pizza");
        result.Price.Should().Be(45.90m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductIsInactive()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(InactiveProduct);

        var result = await _sut.GetByIdAsync(3);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProduct()
    {
        var product = new Product
        {
            Name = "Hambúrguer",
            Price = 32.90m,
            Category = "Lanches",
            IsActive = true
        };

        _productRepositoryMock
            .Setup(r => r.AddAsync(product))
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(product);

        result.Name.Should().Be("Hambúrguer");
        result.Price.Should().Be(32.90m);

        _productRepositoryMock.Verify(r => r.AddAsync(product), Times.Once);
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ImportFromTheMealDBAsync_ShouldImportProducts()
    {
        _productRepositoryMock
            .Setup(r => r.GetExistingExternalIdsAsync())
            .ReturnsAsync(new HashSet<string>());

        _productRepositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>()))
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var imported = await _sut.ImportFromTheMealDBAsync(2);

        imported.Should().Be(1);

        _productRepositoryMock.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<Product>>(p =>
                p.Count() == 1 &&
                p.All(x => x.IsActive && x.ExternalId == "52772"))),
            Times.Once);

        _productRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ImportFromTheMealDBAsync_ShouldSkipExistingExternalIds()
    {
        _productRepositoryMock
            .Setup(r => r.GetExistingExternalIdsAsync())
            .ReturnsAsync(new HashSet<string> { "52772" });

        var imported = await _sut.ImportFromTheMealDBAsync(1);

        imported.Should().Be(0);

        _productRepositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>()),
            Times.Never);

        _productRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ImportFromTheMealDBAsync_ShouldReturnZero_WhenQuantityIsInvalid()
    {
        var imported = await _sut.ImportFromTheMealDBAsync(0);

        imported.Should().Be(0);

        _productRepositoryMock.Verify(
            r => r.GetExistingExternalIdsAsync(),
            Times.Never);
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
