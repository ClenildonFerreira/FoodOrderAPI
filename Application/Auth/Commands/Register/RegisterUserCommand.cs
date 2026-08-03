using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Domain.Common;
using MediatR;

namespace FoodOrderAPI.Application.Auth.Commands.Register;

public class RegisterCommand : IRequest<Result<UserDto>>
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}