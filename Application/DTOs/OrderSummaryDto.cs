namespace FoodOrderAPI.Application.DTOs;

public class OrderSummaryDto
{
    public int Received { get; set; }
    public int Preparing { get; set; }
    public int Ready { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
    public int Total => Received + Preparing + Ready + Delivered + Cancelled;
}