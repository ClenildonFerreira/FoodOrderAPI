namespace FoodOrderAPI.Application.Products.Queries.GetProductById;

public class GetProductByIdQuery
{
    public int Id { get; set; }

    public GetProductByIdQuery(int id)
    {
        Id = id;
    }
}