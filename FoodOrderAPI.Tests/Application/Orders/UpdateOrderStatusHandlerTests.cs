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
        Id = Guid.NewGuid(),
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
        var order = CreateOrder(Guid.NewGuid(), "Carlos", null, OrderType.Delivery);

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            Dto = new FoodOrderAPI.Application.DTOs.UpdateOrderStatusDto { Status = FoodOrderAPI.Application.DTOs.OrderStatusDto.Preparing, Notes = "Em preparo" }
        };

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Preparing");
        result.Value.StatusHistory.Should().HaveCount(2);
        result.Value.StatusHistory.Last().Notes.Should().Be("Em preparo");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTransitionIsInvalid()
    {
        var order = CreateOrder(Guid.NewGuid(), "Carlos", null, OrderType.Delivery);

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(order);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            Dto = new FoodOrderAPI.Application.DTOs.UpdateOrderStatusDto { Status = FoodOrderAPI.Application.DTOs.OrderStatusDto.Delivered }
        };

        var result = await _sut.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.IsNotFound.Should().BeFalse();
        result.Error.Should().Contain("Não é permitido");
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            Dto = new FoodOrderAPI.Application.DTOs.UpdateOrderStatusDto { Status = FoodOrderAPI.Application.DTOs.OrderStatusDto.Preparing }
        };

        var result = await _sut.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.IsNotFound.Should().BeTrue();
        result.Error.Should().Contain("não encontrado");
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    private static Order CreateOrder(
        Guid id,
        string customerName,
        string? tableNumber,
        OrderType type)
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

        var order = Order.Create(customerName, tableNumber, type, items).Value;
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, id);
        return order;
    }
}

