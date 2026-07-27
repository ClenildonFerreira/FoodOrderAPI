using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodOrderAPI.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace FoodOrderAPI.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;

    private static readonly Dictionary <string, string> Users = new()
    {
        {"admin", "admin123"},
        {"garcom", "garcom123"},
        {"cozinha", "cozinha123"}
    };

    public AuthService(IConfiguration config)
    {
        _config = config;
    
    }

    public LoginResponseDto? Login(LoginDto dto)
    {
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            return null;

        if (!Users.ContainsKey(dto.Username) || Users[dto.Username] != dto.Password)
            return null;

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(_config["Jwt:ExpirationMinutes"] ?? "120"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, dto.Username.ToLower()), 
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"]!,
            audience: _config["Jwt:Audience"]!,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
            
}