using FoodOrderAPI.Application.DTOs;

namespace FoodOrderAPI.Application.Interfaces;

public interface IAuthService
{
    LoginResponseDto? Login(LoginDto dto);
}