using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Commands.UpdateOrderStatus;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Orders;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly UpdateOrderStatusHandler _sut;

    private static readonly Product ActivePizza = new()
    {
        Id = 1,
        Name = "Pizza",
        Price = 45.90m,
        IsActive = true
    };

    public UpdateOrderStatusHandlerTests()
    {
        _sut = new UpdateOrderStatusHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdate_WhenTransitionIsValid()
    {
        var order = CreateOrder(
            id: 1,
            customerName: "Carlos",
            tableNumber: null,
            type: OrderType.Delivery);

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 1,
            Status = (int)OrderStatus.Preparing,
            Notes = "Em preparo"
        };

        var result = await _sut.Handle(command);

        result.Should().NotBeNull();
        result!.Status.Should().Be("Preparing");
        result.StatusHistory.Should().HaveCount(2);
        result.StatusHistory.Last().Status.Should().Be("Preparing");
        result.StatusHistory.Last().Notes.Should().Be("Em preparo");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTransitionIsInvalid()
    {
        var order = CreateOrder(
            id: 1,
            customerName: "Carlos",
            tableNumber: null,
            type: OrderType.Delivery);

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 1,
            Status = (int)OrderStatus.Delivered
        };

        var act = async () => await _sut.Handle(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Não é permitido*");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenOrderNotFound()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(999))
            .ReturnsAsync((Order?)null);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 999,
            Status = (int)OrderStatus.Preparing
        };

        var result = await _sut.Handle(command);

        result.Should().BeNull();
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    private static Order CreateOrder(
        int id,
        string customerName,
        string? tableNumber,
        OrderType type)
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

        var order = new Order(customerName, tableNumber, type, items);
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, id);
        return order;
    }
}
