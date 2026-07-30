using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Services;
using FoodOrderAPI.Domain.Entities;
using FoodOrderAPI.Infrastructure.Data;
using FoodOrderAPI.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Tests.Application.Services;

public class OrderServiceTests
{
    private OrderService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        context.Products.AddRange(
            new Product { Id = 1, Name = "Pizza", Price = 45.90m, IsActive = true },
            new Product { Id = 2, Name = "Refrigerante", Price = 8.50m, IsActive = true },
            new Product { Id = 3, Name = "Prato Inativo", Price = 30.00m, IsActive = false }
        );
        context.SaveChanges();

        var orderRepository = new OrderRepository(context);
        var productRepository = new ProductRepository(context);
        return new OrderService(orderRepository, productRepository);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCustomerNameIsEmpty()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "",
            Type = OrderTypeDto.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cliente*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNoItems()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Table,
            TableNumber = "10",
            Items = new()
        };

        var act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*item*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenQuantityIsZero()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Table,
            TableNumber = "10",
            Items = new() { new() { ProductId = 1, Quantity = 0 } }
        };

        var act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*quantidade*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTableOrderWithoutTableNumber()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Table,
            TableNumber = null,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*mesa*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductIsInactive()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 3, Quantity = 1 } }
        };

        var act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*inativo*");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateOrder_WhenDataIsValid()
    {
        var service = CreateService();

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

        var result = await service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.CustomerName.Should().Be("Maria Silva");
        result.Status.Should().Be("Received");
        result.Total.Should().Be(45.90m * 2 + 8.50m);
        result.Items.Should().HaveCount(2);

        result.StatusHistory.Should().HaveCount(1);
        result.StatusHistory[0].Status.Should().Be("Received");
        result.StatusHistory[0].Notes.Should().Be("Pedido criado");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdate_WhenTransitionIsValid()
    {
        var service = CreateService();

        var createDto = new CreateOrderDto
        {
            CustomerName = "Carlos",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var order = await service.CreateAsync(createDto);

        var updateDto = new UpdateOrderStatusDto
        {
            Status = OrderStatusDto.Preparing,
            Notes = "Em preparo"
        };

        var result = await service.UpdateStatusAsync(order.Id, updateDto);

        result.Should().NotBeNull();
        result!.StatusHistory.Should().HaveCount(2);
        result.StatusHistory.Last().Status.Should().Be("Preparing");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenTransitionIsInvalid()
    {
        var service = CreateService();

        var createDto = new CreateOrderDto
        {
            CustomerName = "Carlos",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var order = await service.CreateAsync(createDto);

        var updateDto = new UpdateOrderStatusDto
        {
            Status = OrderStatusDto.Delivered
        };

        var act = async () => await service.UpdateStatusAsync(order.Id, updateDto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Não é permitido*");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldReturnNull_WhenOrderNotFound()
    {
        var service = CreateService();

        var updateDto = new UpdateOrderStatusDto
        {
            Status = OrderStatusDto.Preparing
        };

        var result = await service.UpdateStatusAsync(999, updateDto);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
    {
        var service = CreateService();

        var createDto = new CreateOrderDto
        {
            CustomerName = "Ana",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var created = await service.CreateAsync(createDto);

        var result = await service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.CustomerName.Should().Be("Ana");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var service = CreateService();

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOrders()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "Pedro",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        await service.CreateAsync(dto);

        var result = await service.GetAllAsync();

        result.Items.Should().HaveCount(1);
        result.Items[0].CustomerName.Should().Be("Pedro");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "João",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        var order = await service.CreateAsync(dto);

        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusDto
        {
            Status = OrderStatusDto.Preparing
        });

        var preparing = await service.GetAllAsync(OrderStatus.Preparing);
        var received = await service.GetAllAsync(OrderStatus.Received);

        preparing.Items.Should().HaveCount(1);
        received.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCounts()
    {
        var service = CreateService();

        var dto = new CreateOrderDto
        {
            CustomerName = "Teste",
            Type = OrderTypeDto.Delivery,
            Items = new() { new() { ProductId = 1, Quantity = 1 } }
        };

        await service.CreateAsync(dto);

        var summary = await service.GetSummaryAsync();

        summary.Received.Should().Be(1);
        summary.Total.Should().Be(1);
    }
}
