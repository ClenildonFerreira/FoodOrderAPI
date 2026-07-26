using FoodOrderAPI.Data;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Tests.Services;

public class OrderServiceTests
{
    private AppDbContext CreateInMemoryContext()
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

        return context;
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCustomerNameIsEmpty()
    {
        var service = new OrderService(CreateInMemoryContext());

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
        var service = new OrderService(CreateInMemoryContext());

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
        var service = new OrderService(CreateInMemoryContext());

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
        var service = new OrderService(CreateInMemoryContext());

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
        var service = new OrderService(CreateInMemoryContext());

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
        var service = new OrderService(CreateInMemoryContext());

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
    }
}