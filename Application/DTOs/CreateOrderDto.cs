namespace FoodOrderAPI.Application.DTOs;

public class CreateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string? TableNumber { get; set; }
    public OrderTypeDto Type { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public enum OrderTypeDto
{
    Table = 1,
    Delivery = 2,
}