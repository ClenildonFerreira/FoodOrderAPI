using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FoodOrderAPI.Tests.Application.Services;

public class AuthServiceTests
{
    private AuthService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "FoodOrderAPI_SuperSecretKey_Minimo32Caracteres!",
                ["Jwt:Issuer"] = "FoodOrderAPI",
                ["Jwt:Audience"] = "FoodOrderAPI",
                ["Jwt:ExpirationMinutes"] = "120"
            })
            .Build();

        return new AuthService(config);
    }

    [Fact]
    public void Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var service = CreateService();

        var result = service.Login(new LoginDto
        {
            Username = "admin",
            Password = "admin123"
        });

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Login_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        var service = CreateService();

        var result = service.Login(new LoginDto
        {
            Username = "admin",
            Password = "senha_errada"
        });

        result.Should().BeNull();
    }

    [Fact]
    public void Login_ShouldReturnNull_WhenUsernameIsEmpty()
    {
        var service = CreateService();

        var result = service.Login(new LoginDto
        {
            Username = "",
            Password = "admin123"
        });

        result.Should().BeNull();
    }
}
