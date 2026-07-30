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
        Id = 1,
        Name = "Pizza",
        Price = 45.90m,
        IsActive = true
    };

    private static readonly Product ActiveDrink = new()
    {
        Id = 2,
        Name = "Refrigerante",
        Price = 8.50m,
        IsActive = true
    };

    private static readonly Product InactiveProduct = new()
    {
        Id = 3,
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
    public async Task Handle_ShouldThrow_WhenCustomerNameIsEmpty()
    {
        SetupActiveProduct(1, ActivePizza);

        var command = new CreateOrderCommand
        {
            CustomerName = "",
            Type = (int)OrderType.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var act = async () => await _sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cliente*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNoItems()
    {
        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Table,
            TableNumber = "10",
            Items = new()
        };

        var act = async () => await _sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*item*");

        _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenQuantityIsZero()
    {
        SetupActiveProduct(1, ActivePizza);

        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = 1, Quantity = 0 } }
        };

        var act = async () => await _sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*quantidade*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTableOrderWithoutTableNumber()
    {
        SetupActiveProduct(1, ActivePizza);

        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Table,
            TableNumber = null,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var act = async () => await _sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*mesa*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductIsInactive()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(InactiveProduct);

        var command = new CreateOrderCommand
        {
            CustomerName = "João",
            Type = (int)OrderType.Delivery,
            Items = new() { new() { ProductId = 3, Quantity = 1 } }
        };

        var act = async () => await _sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*inativo*");
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenDataIsValid()
    {
        SetupActiveProduct(1, ActivePizza);
        SetupActiveProduct(2, ActiveDrink);

        Order? capturedOrder = null;

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                SetOrderId(order, 10);
                AttachProducts(order);
                capturedOrder = order;
            })
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(10))
            .ReturnsAsync(() => capturedOrder);

        var command = new CreateOrderCommand
        {
            CustomerName = "Maria Silva",
            Type = (int)OrderType.Table,
            TableNumber = "12",
            Items = new()
            {
                new() { ProductId = 1, Quantity = 2 },
                new() { ProductId = 2, Quantity = 1 }
            }
        };

        var result = await _sut.Handle(command);

        result.Should().NotBeNull();
        result.CustomerName.Should().Be("Maria Silva");
        result.Status.Should().Be("Received");
        result.Total.Should().Be(45.90m * 2 + 8.50m);
        result.Items.Should().HaveCount(2);
        result.StatusHistory.Should().HaveCount(1);
        result.StatusHistory[0].Status.Should().Be("Received");
        result.StatusHistory[0].Notes.Should().Be("Pedido criado");

        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _orderRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(10), Times.Once);
    }

    private void SetupActiveProduct(int id, Product product)
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(product);
    }

    private static void SetOrderId(Order order, int id)
    {
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, id);
    }

    private static void AttachProducts(Order order)
    {
        foreach (var item in order.Items)
        {
            item.Product = item.ProductId switch
            {
                1 => ActivePizza,
                2 => ActiveDrink,
                3 => InactiveProduct,
                _ => item.Product
            };
        }
    }
}
