using FoodOrderAPI.DTOs;

namespace FoodOrderAPI.Services;

public interface IAuthService
{
    LoginResponseDto? Login(LoginDto dto);
}