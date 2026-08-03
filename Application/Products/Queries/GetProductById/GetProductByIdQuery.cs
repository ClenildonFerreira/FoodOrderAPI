using MediatR;
using FoodOrderAPI.Application.DTOs;

namespace FoodOrderAPI.Application.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto?>
{
    public int Id { get; set; }

    public GetProductByIdQuery(int id)
    {
        Id = id;
    }
}