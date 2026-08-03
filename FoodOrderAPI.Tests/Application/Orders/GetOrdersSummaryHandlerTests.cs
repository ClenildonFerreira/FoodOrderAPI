using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Queries.GetOrdersSummary;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Orders;

public class GetOrdersSummaryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly GetOrdersSummaryHandler _sut;

    public GetOrdersSummaryHandlerTests()
    {
        _sut = new GetOrdersSummaryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCounts()
    {
        _orderRepositoryMock
            .Setup(r => r.GetSummaryAsync())
            .ReturnsAsync(new Dictionary<OrderStatus, int>
            {
                [OrderStatus.Received] = 1,
                [OrderStatus.Preparing] = 2
            });

        var summary = await _sut.Handle(new GetOrdersSummaryQuery(), default);

        summary.Received.Should().Be(1);
        summary.Preparing.Should().Be(2);
        summary.Ready.Should().Be(0);
        summary.Delivered.Should().Be(0);
        summary.Cancelled.Should().Be(0);
        summary.Total.Should().Be(3);
    }
}
