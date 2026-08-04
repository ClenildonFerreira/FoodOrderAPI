using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Domain.Common;
using MediatR;

namespace FoodOrderAPI.Application.Auth.Commands.Register;

public class RegisterUserCommand : IRequest<Result<UserDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Role { get; set; }
}