namespace FoodOrderAPI.Application.DTOs;

public class UpdateOrderStatusDto
{
    public OrderStatusDto Status { get; set; }
    public string? Notes { get; set; }
}

public enum OrderStatusDto
{
    Received = 1,
    Preparing = 2,
    Ready = 3,
    Delivered = 4,
    Cancelled = 5,
}