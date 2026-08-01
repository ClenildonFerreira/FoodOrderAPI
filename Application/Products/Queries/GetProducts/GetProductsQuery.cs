namespace FoodOrderAPI.Application.Products.Queries.GetProducts;

public class GetProductsQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}