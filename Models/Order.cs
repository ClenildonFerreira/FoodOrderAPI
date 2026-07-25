namespace FoodOrderAPI.Models;
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? TableNumber { get; set; }
        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Received;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }    

        public List<OrderItem> Items { get; set; } = new();
        public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    }


    public enum OrderType
    {
        Table = 1,
        Delivery = 2,
    }

    public enum OrderStatus
    {
        Received = 1, 
        Preparing = 2,
        Ready = 3,
        Delivered = 4,
        Cancelled = 5,
    }