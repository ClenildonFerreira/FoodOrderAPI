using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Products.Commands.ImportProducts;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Products;

public class ImportProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly ImportProductsHandler _sut;

    public ImportProductsHandlerTests()
    {
        _httpClientFactoryMock
            .Setup(f => f.CreateClient("TheMealDB"))
            .Returns(new HttpClient(new FakeHttpMessageHandler())
            {
                BaseAddress = new Uri("https://www.themealdb.com/api/json/v1/1/")
            });

        _sut = new ImportProductsHandler(
            _productRepositoryMock.Object,
            _httpClientFactoryMock.Object,
            NullLogger<ImportProductsHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldImportProducts()
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

        var result = await _sut.Handle(new ImportProductsCommand { Quantity = 2 }, default);

        // Same fake meal id for both requests → only 1 unique product
        result.Imported.Should().Be(1);
        result.Skipped.Should().Be(1); // second one is duplicate
        result.FailedHttp.Should().Be(0);

        _productRepositoryMock.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<Product>>(p =>
                p.Count() == 1 &&
                p.All(x => x.IsActive && x.ExternalId == "52772"))),
            Times.Once);

        _productRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipExistingExternalIds()
    {
        _productRepositoryMock
            .Setup(r => r.GetExistingExternalIdsAsync())
            .ReturnsAsync(new HashSet<string> { "52772" });

        var result = await _sut.Handle(new ImportProductsCommand { Quantity = 1 }, default);

        result.Imported.Should().Be(0);
        result.Skipped.Should().Be(1);

        _productRepositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>()),
            Times.Never);

        _productRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenQuantityIsZeroOrNegative()
    {
        var result = await _sut.Handle(new ImportProductsCommand { Quantity = 0 }, default);

        result.Imported.Should().Be(0);
        result.Skipped.Should().Be(0);
        result.FailedHttp.Should().Be(0);

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
