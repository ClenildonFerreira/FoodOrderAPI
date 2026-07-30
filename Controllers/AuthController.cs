using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponseDto> Login([FromBody] LoginDto dto)
    {
        var result = _authService.Login(dto);

        if (result is null)
            return Unauthorized(new { error = "Usuário ou senha inválidos." });

        return Ok(result);
    }
}