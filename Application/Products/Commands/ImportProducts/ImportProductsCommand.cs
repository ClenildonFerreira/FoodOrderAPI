using MediatR;

namespace FoodOrderAPI.Application.Products.Commands.ImportProducts;

public class ImportProductsCommand : IRequest<int>
{
    public int Quantity { get; set; } = 10;
}
