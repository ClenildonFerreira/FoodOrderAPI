using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Commands.CreateOrder;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Orders;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly CreateOrderHandler _sut;

    private static readonly Product ActivePizza = new()
    {
        Id = Guid.NewGuid(),
        Name = "Pizza",
        Price = 45.90m,
        IsActive = true
    };

    private static readonly Product ActiveDrink = new()
    {
        Id = Guid.NewGuid(),
        Name = "Refrigerante",
        Price = 8.50m,
        IsActive = true
    };

    private static readonly Product InactiveProduct = new()
    {
        Id = Guid.NewGuid(),
        Name = "Prato Inativo",
        Price = 30.00m,
        IsActive = false
    };

    public CreateOrderHandlerTests()
    {
        _sut = new CreateOrderHandler(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCustomerNameIsEmpty()
    {
        SetupActiveProduct(ActivePizza.Id, ActivePizza);

        var command = new CreateOrderCommand
        {
            CustomerName = "",
            Type = (int)OrderType.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = ActivePizza.Id, Quantity = 1 } }
        };

        var result = await _sut.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cliente");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNoItems()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Table,
            TableNumber = "10",
            Items = new()
        };

        var result = await _sut.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("item");
        _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenQuantityIsZero()
    {
        SetupActiveProduct(ActivePizza.Id, ActivePizza);

        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = ActivePizza.Id, Quantity = 0 } }
        };

        var result = await _sut.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("quantidade");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTableOrderWithoutTableNumber()
    {
        SetupActiveProduct(ActivePizza.Id, ActivePizza);

        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Table,
            TableNumber = null,
            Items = new() { new() { ProductId = ActivePizza.Id, Quantity = 1 } }
        };

        var result = await _sut.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("mesa");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenProductIsInactive()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(InactiveProduct);

        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Delivery,
            Items = new() { new() { ProductId = ActivePizza.Id, Quantity = 1 } }
        };

        var result = await _sut.Handle(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("inativo");
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenDataIsValid()
    {
        SetupActiveProduct(ActivePizza.Id, ActivePizza);
        SetupActiveProduct(ActiveDrink.Id, ActiveDrink);

        Order? capturedOrder = null;

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                SetOrderId(order, Guid.NewGuid());
                AttachProducts(order);
                capturedOrder = order;
            })
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => capturedOrder);

        var command = new CreateOrderCommand
        {
            CustomerName = "Maria Silva",
            Type = (int)OrderType.Table,
            TableNumber = "12",
            Items = new()
            {
                new() { ProductId = ActivePizza.Id, Quantity = 2 },
                new() { ProductId = ActiveDrink.Id, Quantity = 1 }
            }
        };

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerName.Should().Be("Maria Silva");
        result.Value.Status.Should().Be("Received");
        result.Value.Total.Should().Be(45.90m * 2 + 8.50m);
        result.Value.Items.Should().HaveCount(2);
        result.Value.StatusHistory.Should().HaveCount(1);
        result.Value.StatusHistory[0].Notes.Should().Be("Pedido criado");

        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    private void SetupActiveProduct(Guid id, Product product)
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(product);
    }

    private static void SetOrderId(Order order, Guid id)
    {
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, id);
    }

    private static void AttachProducts(Order order)
    {
        foreach (var item in order.Items)
        {
            if (item.ProductId == ActivePizza.Id) item.Product = ActivePizza;
            else if (item.ProductId == ActiveDrink.Id) item.Product = ActiveDrink;
            else if (item.ProductId == InactiveProduct.Id) item.Product = InactiveProduct;
        }
    }
}

