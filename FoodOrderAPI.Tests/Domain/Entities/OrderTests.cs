using FoodOrderAPI.Domain.Entities;
using FluentAssertions;

namespace FoodOrderAPI.Tests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Create_ShouldFail_WhenCustomerNameIsEmpty()
    {
        var items = new List<OrderItem> { new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m } };
        var result = Order.Create("", "10", OrderType.Table, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cliente");
    }

    [Fact]
    public void Create_ShouldFail_WhenNoItems()
    {
        var result = Order.Create("João", "10", OrderType.Table, new List<OrderItem>());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("item");
    }

    [Fact]
    public void Create_ShouldFail_WhenQuantityIsZero()
    {
        var items = new List<OrderItem> { new() { ProductId = Guid.NewGuid(), Quantity = 0, UnitPrice = 10m } };
        var result = Order.Create("João", "10", OrderType.Table, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("quantidade");
    }

    [Fact]
    public void Create_ShouldFail_WhenTableOrderWithoutTableNumber()
    {
        var items = new List<OrderItem> { new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m } };
        var result = Order.Create("João", null, OrderType.Table, items);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("mesa");
    }

    [Fact]
    public void Create_ShouldCreateOrderAndCalculateTotal_WhenValid()
    {
        var items = new List<OrderItem>
        {
            new() { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 45.90m },
            new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 8.50m }
        };

        var result = Order.Create("Maria Silva", "12", OrderType.Table, items);

        result.IsSuccess.Should().BeTrue();
        var order = result.Value;
        order.CustomerName.Should().Be("Maria Silva");
        order.TableNumber.Should().Be("12");
        order.Type.Should().Be(OrderType.Table);
        order.Status.Should().Be(OrderStatus.Received);
        order.Total.Should().Be(45.90m * 2 + 8.50m);
        order.Items.Should().HaveCount(2);
        order.StatusHistory.Should().HaveCount(1);
        order.StatusHistory[0].Status.Should().Be(OrderStatus.Received);
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndAddHistory_WhenTransitionIsValid()
    {
        var items = new List<OrderItem> { new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m } };
        var order = Order.Create("Carlos", null, OrderType.Delivery, items).Value;

        var result = order.ChangeStatus(OrderStatus.Preparing, "Em preparo");

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Preparing);
        order.StatusHistory.Should().HaveCount(2);
        order.StatusHistory.Last().Status.Should().Be(OrderStatus.Preparing);
        order.StatusHistory.Last().Notes.Should().Be("Em preparo");
    }

    [Fact]
    public void ChangeStatus_ShouldFail_WhenTransitionIsInvalid()
    {
        var items = new List<OrderItem> { new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m } };
        var order = Order.Create("Carlos", null, OrderType.Delivery, items).Value;

        var result = order.ChangeStatus(OrderStatus.Delivered);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Não é permitido");
        order.Status.Should().Be(OrderStatus.Received);
    }
}
