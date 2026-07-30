using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler
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

    public async Task<OrderDto> Handle(CreateOrderCommand command)
    {
        if (command.Items is null || !command.Items.Any())
            throw new ArgumentException("O pedido deve conter pelo menos 1 item.");

        var items = new List<OrderItem>();

        foreach (var itemDto in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

            if (product is null)
                throw new ArgumentException($"Produto com ID {itemDto.ProductId} não encontrado.");

            if (!product.IsActive)
                throw new ArgumentException($"O Produto {product.Name} está inativo e não pode ser adicionado ao pedido.");

            items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });
        }

        var order = new Order(
            command.CustomerName,
            command.TableNumber,
            (OrderType)command.Type,
            items
        );

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        var createdOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id)
            ?? throw new Exception("Erro ao criar pedido.");

        return MapToDto(createdOrder);
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