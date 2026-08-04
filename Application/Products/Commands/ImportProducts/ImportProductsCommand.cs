using MediatR;

namespace FoodOrderAPI.Application.Products.Commands.ImportProducts;

public class ImportProductsCommand : IRequest<ImportProductsResult>
{
    public int Quantity { get; set; } = 10;
}

public record ImportProductsResult(
    int Imported,
    int Skipped,
    int FailedHttp,
    long DurationMs);
