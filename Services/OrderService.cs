using FoodOrderAPI.Data;
using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        // validações básicas
        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            throw new ArgumentException("Nome do cliente é obrigatório.");

        if (dto.Items is null || !dto.Items.Any())
            throw new ArgumentException("O pedido deve conter pelo menos 1 item.");

        if (dto.Items.Any(i => i.Quantity <= 0))
            throw new ArgumentException("A quantidade de cada item deve ser maior que zero.");

        // validaçao de messa para pedidos de salão 
        if (dto.Type == OrderTypeDto.Table && string.IsNullOrWhiteSpace(dto.TableNumber))
            throw new ArgumentException("Número da mesa é obrigatório para pedidos de salão.");

        var order = new Order
        {
            CustomerName = dto.CustomerName.Trim(),
            TableNumber = dto.TableNumber?.Trim(),
            Type = (OrderType)dto.Type,
            Status = OrderStatus.Received,
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;

        foreach (var itemDto in dto.Items)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            
            if (product is null)
                throw new ArgumentException($"Produto com ID {itemDto.ProductId} não encontrado.");
            if (!product.IsActive)
                throw new ArgumentException($"O Produto {product.Name} está inativo e não pode ser adicionado ao pedido.");

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            };

            total += product.Price * itemDto.Quantity;
            order.Items.Add(orderItem);
        }

        order.Total = total;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Received,
            Notes = "Pedido criado"
        });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(order.Id) ?? throw new Exception("Erro ao criar pedido");
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return null;

        return MapToDto(order);
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

        var query = _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.StatusHistory)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (type.HasValue)
            query = query.Where(o => o.Type == type.Value);

        var totalItems = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
        var orders = await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new OrderSummaryDto
        {
            Received = orders.FirstOrDefault(x => x.Status == OrderStatus.Received)?.Count ?? 0,
            Preparing = orders.FirstOrDefault(x => x.Status == OrderStatus.Preparing)?.Count ?? 0,
            Ready = orders.FirstOrDefault(x => x.Status == OrderStatus.Ready)?.Count ?? 0,
            Delivered = orders.FirstOrDefault(x => x.Status == OrderStatus.Delivered)?.Count ?? 0,
            Cancelled = orders.FirstOrDefault(x => x.Status == OrderStatus.Cancelled)?.Count ?? 0
        };
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return null;

        var newStatus = (OrderStatus)dto.Status;

        if (!OrderStatusTransition.CanTransition(order.Status, newStatus))
        {
            throw new InvalidOperationException(
                OrderStatusTransition.GetErrorMessage(order.Status, newStatus)
            );
        }

        order.Status = newStatus;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = newStatus,
            Notes = dto.Notes ?? $"Status alterado para {newStatus}"
        });

        await _context.SaveChangesAsync();
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
            .Select(h => new OrderStatusHistoryDto {
                 Status = h.Status.ToString(),
                 ChangedAt = h.ChangedAt,
                 Notes = h.Notes
                }).ToList()
        };
    }
}