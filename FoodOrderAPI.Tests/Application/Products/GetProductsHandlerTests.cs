using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Products.Queries.GetProducts;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Products;

public class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly GetProductsHandler _sut;

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

    public GetProductsHandlerTests()
    {
        _sut = new GetProductsHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveProducts()
    {
        _productRepositoryMock
            .Setup(r => r.GetActivePagedAsync(1, 10))
            .ReturnsAsync((new List<Product> { ActivePizza, ActiveJuice }, 2));

        var result = await _sut.Handle(new GetProductsQuery
        {
            Page = 1,
            PageSize = 10
        });

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.Name == "Pizza" || p.Name == "Suco");
        result.TotalItems.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldNormalizePaging_WhenInvalid()
    {
        _productRepositoryMock
            .Setup(r => r.GetActivePagedAsync(1, 10))
            .ReturnsAsync((new List<Product>(), 0));

        var result = await _sut.Handle(new GetProductsQuery
        {
            Page = 0,
            PageSize = 0
        });

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        _productRepositoryMock.Verify(r => r.GetActivePagedAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCapPageSize_WhenTooLarge()
    {
        _productRepositoryMock
            .Setup(r => r.GetActivePagedAsync(1, 50))
            .ReturnsAsync((new List<Product>(), 0));

        var result = await _sut.Handle(new GetProductsQuery
        {
            Page = 1,
            PageSize = 100
        });

        result.PageSize.Should().Be(50);

        _productRepositoryMock.Verify(r => r.GetActivePagedAsync(1, 50), Times.Once);
    }
}
