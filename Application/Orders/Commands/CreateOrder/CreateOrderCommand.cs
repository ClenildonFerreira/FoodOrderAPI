namespace FoodOrderAPI.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommand
{
    public string CustomerName { get; set; } = string.Empty;
    public string? TableNumber { get; set; }
    public int Type { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
