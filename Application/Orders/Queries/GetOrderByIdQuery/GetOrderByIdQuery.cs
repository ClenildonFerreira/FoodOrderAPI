namespace FoodOrderAPI.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery
{
    public int Id { get; set; }

    public GetOrderByIdQuery(int id)
    {
        Id = id;
    }
}