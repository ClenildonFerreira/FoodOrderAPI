using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Domain.Common;
using MediatR;

namespace FoodOrderAPI.Application.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<LoginResponseDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}