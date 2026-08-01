using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Products.Queries.GetProductById;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Products;

public class GetProductByIdHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly GetProductByIdHandler _sut;

    private static readonly Product ActivePizza = new()
    {
        Id = 1,
        Name = "Pizza",
        Price = 45.90m,
        IsActive = true,
        Category = "Massas"
    };

    private static readonly Product InactiveProduct = new()
    {
        Id = 3,
        Name = "Prato Antigo",
        Price = 20.00m,
        IsActive = false
    };

    public GetProductByIdHandlerTests()
    {
        _sut = new GetProductByIdHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenExistsAndActive()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(ActivePizza);

        var result = await _sut.Handle(new GetProductByIdQuery(1));

        result.Should().NotBeNull();
        result!.Name.Should().Be("Pizza");
        result.Price.Should().Be(45.90m);
        result.Category.Should().Be("Massas");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenProductIsInactive()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(InactiveProduct);

        var result = await _sut.Handle(new GetProductByIdQuery(3));

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotExists()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var result = await _sut.Handle(new GetProductByIdQuery(999));

        result.Should().BeNull();
    }
}
