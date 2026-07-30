using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Services;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCounts()
    {
        _orderRepositoryMock
            .Setup(r => r.GetSummaryAsync())
            .ReturnsAsync(new Dictionary<OrderStatus, int>
            {
                [OrderStatus.Received] = 1,
                [OrderStatus.Preparing] = 2
            });

        var summary = await _sut.GetSummaryAsync();

        summary.Received.Should().Be(1);
        summary.Preparing.Should().Be(2);
        summary.Total.Should().Be(3);
    }
}
