using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Queries.GetOrderById;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Orders;

public class GetOrderByIdHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly GetOrderByIdHandler _sut;

    private static readonly Product ActivePizza = new()
    {
        Id = Guid.NewGuid(),
        Name = "Pizza",
        Price = 45.90m,
        IsActive = true
    };

    public GetOrderByIdHandlerTests()
    {
        _sut = new GetOrderByIdHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrder_WhenExists()
    {
        var order = CreateOrder(Guid.NewGuid(), "Ana");

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(order);

        var result = await _sut.Handle(new GetOrderByIdQuery(Guid.NewGuid()), default);

        result.Should().NotBeNull();
        result!.CustomerName.Should().Be("Ana");
        result.Id.Should().NotBeEmpty();
        result.Items.Should().ContainSingle(i => i.ProductName == "Pizza");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotExists()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        var result = await _sut.Handle(new GetOrderByIdQuery(Guid.NewGuid()), default);

        result.Should().BeNull();
    }

    private static Order CreateOrder(Guid id, string customerName)
    {
        var items = new List<OrderItem>
        {
            new()
            {
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 45.90m,
                Product = ActivePizza
            }
        };

        var order = Order.Create(customerName, null, OrderType.Delivery, items).Value;
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, id);
        return order;
    }
}
