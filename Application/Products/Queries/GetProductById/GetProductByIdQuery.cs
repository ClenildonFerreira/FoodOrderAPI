using MediatR;
using FoodOrderAPI.Application.DTOs;

namespace FoodOrderAPI.Application.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto?>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}