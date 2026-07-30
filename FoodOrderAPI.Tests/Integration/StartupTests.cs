using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FoodOrderAPI.Tests.Integration;

public class StartupTests
{
    [Fact]
    public void Startup_ShouldThrowInvalidOperationException_WhenJwtKeyIsMissing()
    {
        // Act
        var act = () => 
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Production");
                });
            factory.CreateClient();
        };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*A chave JWT (Jwt:Key) não está configurada*");
    }
}
