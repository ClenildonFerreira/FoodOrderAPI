using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Common;
using FoodOrderAPI.Domain.Entities;
using MediatR;

namespace FoodOrderAPI.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.Items is null || !command.Items.Any())
            return Result.Failure<OrderDto>("O pedido deve conter pelo menos 1 item.");

        var items = new List<OrderItem>();

        foreach (var itemDto in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

            if (product is null)
                return Result.Failure<OrderDto>($"Produto com ID {itemDto.ProductId} não encontrado.");

            if (!product.IsActive)
                return Result.Failure<OrderDto>(
                    $"O Produto {product.Name} está inativo e não pode ser adicionado ao pedido.");

            items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });
        }

        var orderResult = Order.Create(
            command.CustomerName,
            command.TableNumber,
            (OrderType)command.Type,
            items);

        if (orderResult.IsFailure)
            return Result.Failure<OrderDto>(orderResult.Error);

        var order = orderResult.Value;

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        var createdOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id);
        if (createdOrder is null)
            return Result.Failure<OrderDto>("Erro ao criar pedido.");

        return Result.Success(MapToDto(createdOrder));
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            TableNumber = order.TableNumber,
            Type = order.Type.ToString(),
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            Total = order.Total,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            StatusHistory = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDto
                {
                    Status = h.Status.ToString(),
                    ChangedAt = h.ChangedAt,
                    Notes = h.Notes
                }).ToList()
        };
    }
}
