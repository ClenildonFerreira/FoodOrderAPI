using FoodOrderAPI.Domain.Entities;
using FluentAssertions;

namespace FoodOrderAPI.Tests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenCustomerNameIsEmpty()
    {
        var items = new List<OrderItem> { new() { ProductId = 1, Quantity = 1, UnitPrice = 10m } };
        var act = () => new Order("", "10", OrderType.Table, items);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cliente*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNoItems()
    {
        var act = () => new Order("João", "10", OrderType.Table, new List<OrderItem>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*item*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsZero()
    {
        var items = new List<OrderItem> { new() { ProductId = 1, Quantity = 0, UnitPrice = 10m } };
        var act = () => new Order("João", "10", OrderType.Table, items);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*quantidade*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTableOrderWithoutTableNumber()
    {
        var items = new List<OrderItem> { new() { ProductId = 1, Quantity = 1, UnitPrice = 10m } };
        var act = () => new Order("João", null, OrderType.Table, items);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*mesa*");
    }

    [Fact]
    public void Constructor_ShouldCreateOrderAndCalculateTotal_WhenValid()
    {
        var items = new List<OrderItem>
        {
            new() { ProductId = 1, Quantity = 2, UnitPrice = 45.90m },
            new() { ProductId = 2, Quantity = 1, UnitPrice = 8.50m }
        };

        var order = new Order("Maria Silva", "12", OrderType.Table, items);

        order.Should().NotBeNull();
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
        var items = new List<OrderItem> { new() { ProductId = 1, Quantity = 1, UnitPrice = 10m } };
        var order = new Order("Carlos", null, OrderType.Delivery, items);

        order.ChangeStatus(OrderStatus.Preparing, "Em preparo");

        order.Status.Should().Be(OrderStatus.Preparing);
        order.StatusHistory.Should().HaveCount(2);
        order.StatusHistory.Last().Status.Should().Be(OrderStatus.Preparing);
        order.StatusHistory.Last().Notes.Should().Be("Em preparo");
    }

    [Fact]
    public void ChangeStatus_ShouldThrow_WhenTransitionIsInvalid()
    {
        var items = new List<OrderItem> { new() { ProductId = 1, Quantity = 1, UnitPrice = 10m } };
        var order = new Order("Carlos", null, OrderType.Delivery, items);

        var act = () => order.ChangeStatus(OrderStatus.Delivered);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Não é permitido*");
    }
}
