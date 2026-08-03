using MediatR;
using FoodOrderAPI.Application.DTOs;

namespace FoodOrderAPI.Application.Products.Queries.GetProducts;

public class GetProductsQuery : IRequest<PagedResultDto<ProductDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}