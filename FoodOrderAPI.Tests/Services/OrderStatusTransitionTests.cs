using FoodOrderAPI.Domain.Entities;
using FoodOrderAPI.Domain.Services;
using FluentAssertions;

namespace FoodOrderAPI.Tests.Services;

public class OrderStatusTransitionTests
{
    [Theory]
    [InlineData(OrderStatus.Received, OrderStatus.Preparing, true)]
    [InlineData(OrderStatus.Received, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Received, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Ready, true)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Ready, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Ready, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Preparing, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Received, false)]
    public void CanTransition_ShouldReturnExpectedResult(
        OrderStatus current,
        OrderStatus next,
        bool expected)
    {
        var result = OrderStatusTransition.CanTransition(current, next);
        result.Should().Be(expected);
    }

    [Fact]
    public void GetErrorMessage_ShouldReturnCorrectMessage()
    {
        var message = OrderStatusTransition.GetErrorMessage(
            OrderStatus.Received,
            OrderStatus.Delivered);

        message.Should().Be("Não é permitido alterar o status de 'Received' para 'Delivered'.");
    }
}