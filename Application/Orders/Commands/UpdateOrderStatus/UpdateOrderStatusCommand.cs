namespace FoodOrderAPI.Application.Orders.Commands.UpdateOrderStatus;


public class UpdateOrderStatusCommand
{
    public int OrderId { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
}