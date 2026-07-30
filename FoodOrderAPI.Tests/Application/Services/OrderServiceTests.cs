using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Services;
using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FoodOrderAPI.Tests.Application.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly OrderService _sut;

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

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();

        _sut = new OrderService(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCustomerNameIsEmpty()
    {
        SetupActiveProduct(1, ActivePizza);

        var dto = new CreateOrderDto
        {
            CustomerName = "",
            Type = OrderTypeDto.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cliente*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNoItems()
    {
        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Table,
            TableNumber = "10",
            Items = new()
        };

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*item*");

        _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenQuantityIsZero()
    {
        SetupActiveProduct(1, ActivePizza);

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = 1, Quantity = 0 } }
        };

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*quantidade*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTableOrderWithoutTableNumber()
    {
        SetupActiveProduct(1, ActivePizza);

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Table,
            TableNumber = null,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*mesa*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductIsInactive()
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(InactiveProduct);

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 3, Quantity = 1 } }
        };

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*inativo*");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateOrder_WhenDataIsValid()
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

        var dto = new CreateOrderDto
        {
            CustomerName = "Maria Silva",
            Type = OrderTypeDto.Table,
            TableNumber = "12",
            Items = new()
            {
                new() { ProductId = 1, Quantity = 2 },
                new() { ProductId = 2, Quantity = 1 }
            }
        };

        var result = await _sut.CreateAsync(dto);

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
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdate_WhenTransitionIsValid()
    {
        var order = CreateOrder(
            id: 1,
            customerName: "Carlos",
            tableNumber: null,
            type: OrderType.Delivery,
            items: new List<OrderItem>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 45.90m,
                    Product = ActivePizza
                }
            });

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var updateDto = new UpdateOrderStatusDto
        {
            Status = OrderStatusDto.Preparing,
            Notes = "Em preparo"
        };

        var result = await _sut.UpdateStatusAsync(1, updateDto);

        result.Should().NotBeNull();
        result!.Status.Should().Be("Preparing");
        result.StatusHistory.Should().HaveCount(2);
        result.StatusHistory.Last().Status.Should().Be("Preparing");
        result.StatusHistory.Last().Notes.Should().Be("Em preparo");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenTransitionIsInvalid()
    {
        var order = CreateOrder(
            id: 1,
            customerName: "Carlos",
            tableNumber: null,
            type: OrderType.Delivery,
            items: new List<OrderItem>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 45.90m,
                    Product = ActivePizza
                }
            });

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(order);

        var updateDto = new UpdateOrderStatusDto
        {
            Status = OrderStatusDto.Delivered
        };

        var act = async () => await _sut.UpdateStatusAsync(1, updateDto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Não é permitido*");

        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldReturnNull_WhenOrderNotFound()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(999))
            .ReturnsAsync((Order?)null);

        var updateDto = new UpdateOrderStatusDto
        {
            Status = OrderStatusDto.Preparing
        };

        var result = await _sut.UpdateStatusAsync(999, updateDto);

        result.Should().BeNull();
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        var order = CreateOrder(
            id: 5,
            customerName: "Ana",
            tableNumber: null,
            type: OrderType.Delivery,
            items: new List<OrderItem>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 45.90m,
                    Product = ActivePizza
                }
            });

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(5))
            .ReturnsAsync(order);

        var result = await _sut.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.CustomerName.Should().Be("Ana");
        result.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(999))
            .ReturnsAsync((Order?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOrders()
    {
        var order = CreateOrder(
            id: 1,
            customerName: "Pedro",
            tableNumber: null,
            type: OrderType.Delivery,
            items: new List<OrderItem>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 45.90m,
                    Product = ActivePizza
                }
            });

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(null, null, 1, 10))
            .ReturnsAsync((new List<Order> { order }, 1));

        var result = await _sut.GetAllAsync();

        result.Items.Should().HaveCount(1);
        result.Items[0].CustomerName.Should().Be("Pedro");
        result.TotalItems.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        var preparingOrder = CreateOrder(
            id: 1,
            customerName: "João",
            tableNumber: null,
            type: OrderType.Delivery,
            items: new List<OrderItem>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 45.90m,
                    Product = ActivePizza
                }
            });
        preparingOrder.ChangeStatus(OrderStatus.Preparing);

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(OrderStatus.Preparing, null, 1, 10))
            .ReturnsAsync((new List<Order> { preparingOrder }, 1));

        _orderRepositoryMock
            .Setup(r => r.GetPagedAsync(OrderStatus.Received, null, 1, 10))
            .ReturnsAsync((new List<Order>(), 0));

        var preparing = await _sut.GetAllAsync(OrderStatus.Preparing);
        var received = await _sut.GetAllAsync(OrderStatus.Received);

        preparing.Items.Should().HaveCount(1);
        received.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCounts()
    {
        _orderRepositoryMock
            .Setup(r => r.GetSummaryAsync())
            .ReturnsAsync(new Dictionary<OrderStatus, int>
            {
                [OrderStatus.Received] = 1
            });

        var summary = await _sut.GetSummaryAsync();

        summary.Received.Should().Be(1);
        summary.Total.Should().Be(1);
    }

    private void SetupActiveProduct(int id, Product product)
    {
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(product);
    }

    private static Order CreateOrder(
        int id,
        string customerName,
        string? tableNumber,
        OrderType type,
        List<OrderItem> items)
    {
        var order = new Order(customerName, tableNumber, type, items);
        SetOrderId(order, id);
        return order;
    }

    private static void SetOrderId(Order order, int id)
    {
        typeof(Order)
            .GetProperty(nameof(Order.Id))!
            .SetValue(order, id);
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
