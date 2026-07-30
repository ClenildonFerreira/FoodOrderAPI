using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        if (dto.Items is null || !dto.Items.Any())
            throw new ArgumentException("O pedido deve conter pelo menos 1 item.");

        var items = new List<OrderItem>();

        foreach (var itemDto in dto.Items)
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
            dto.CustomerName,
            dto.TableNumber,
            (OrderType)dto.Type,
            items
        );

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return await GetByIdAsync(order.Id) ?? throw new Exception("Erro ao criar pedido");
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        return order is null ? null : MapToDto(order);
    }

    public async Task<PagedResultDto<OrderDto>> GetAllAsync(
        OrderStatus? status = null,
        OrderType? type = null,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var (orders, totalItems) = await _orderRepository.GetPagedAsync(status, type, page, pageSize);

        return new PagedResultDto<OrderDto>
        {
            Items = orders.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<OrderSummaryDto> GetSummaryAsync()
    {
        var summary = await _orderRepository.GetSummaryAsync();

        return new OrderSummaryDto
        {
            Received = summary.GetValueOrDefault(OrderStatus.Received),
            Preparing = summary.GetValueOrDefault(OrderStatus.Preparing),
            Ready = summary.GetValueOrDefault(OrderStatus.Ready),
            Delivered = summary.GetValueOrDefault(OrderStatus.Delivered),
            Cancelled = summary.GetValueOrDefault(OrderStatus.Cancelled)
        };
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        if (order is null) return null;

        var newStatus = (OrderStatus)dto.Status;
        order.ChangeStatus(newStatus, dto.Notes);

        await _orderRepository.SaveChangesAsync();
        return MapToDto(order);
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