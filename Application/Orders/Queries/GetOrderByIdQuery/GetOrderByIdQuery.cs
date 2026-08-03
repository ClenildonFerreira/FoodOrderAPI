using MediatR;
using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Domain.Common;

namespace FoodOrderAPI.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery : IRequest<OrderDto?>
{
    public int Id { get; set; }

    public GetOrderByIdQuery(int id)
    {
        Id = id;
    }
}