using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Queries.GetOrders;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Orders;

public class GetOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly GetOrdersHandler _sut;

    private static readonly Product ActivePizza = new()
    {
        Id = 1,
        Name = "Pizza",
        Price = 45.90m,
        IsActive = true
    };

    public GetOrdersHandlerTests()
    {
        _sut = new GetOrdersHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrders()
    {
        var order = CreateOrder(1, "Pedro", OrderStatus.Received);

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(null, null, 1, 10))
            .ReturnsAsync((new List<Order> { order }, 1));

        var result = await _sut.Handle(new GetOrdersQuery
        {
            Page = 1,
            PageSize = 10
        });

        result.Items.Should().HaveCount(1);
        result.Items[0].CustomerName.Should().Be("Pedro");
        result.TotalItems.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus()
    {
        var preparingOrder = CreateOrder(1, "João", OrderStatus.Received);
        preparingOrder.ChangeStatus(OrderStatus.Preparing);

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(OrderStatus.Preparing, null, 1, 10))
            .ReturnsAsync((new List<Order> { preparingOrder }, 1));

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(OrderStatus.Received, null, 1, 10))
            .ReturnsAsync((new List<Order>(), 0));

        var preparing = await _sut.Handle(new GetOrdersQuery
        {
            Status = OrderStatus.Preparing,
            Page = 1,
            PageSize = 10
        });

        var received = await _sut.Handle(new GetOrdersQuery
        {
            Status = OrderStatus.Received,
            Page = 1,
            PageSize = 10
        });

        preparing.Items.Should().HaveCount(1);
        received.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNormalizePaging_WhenInvalid()
    {
        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(null, null, 1, 10))
            .ReturnsAsync((new List<Order>(), 0));

        var result = await _sut.Handle(new GetOrdersQuery
        {
            Page = 0,
            PageSize = 0
        });

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        _orderRepositoryMock.Verify(r => r.GetPagedAsync(null, null, 1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCapPageSize_WhenTooLarge()
    {
        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(null, null, 1, 50))
            .ReturnsAsync((new List<Order>(), 0));

        var result = await _sut.Handle(new GetOrdersQuery
        {
            Page = 1,
            PageSize = 100
        });

        result.PageSize.Should().Be(50);

        _orderRepositoryMock.Verify(r => r.GetPagedAsync(null, null, 1, 50), Times.Once);
    }

    private static Order CreateOrder(int id, string customerName, OrderStatus _)
    {
        var items = new List<OrderItem>
        {
            new()
            {
                ProductId = 1,
                Quantity = 1,
                UnitPrice = 45.90m,
                Product = ActivePizza
            }
        };

        var order = new Order(customerName, null, OrderType.Delivery, items);
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, id);
        return order;
    }
}