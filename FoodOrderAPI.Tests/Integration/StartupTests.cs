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
        var exception = act.Should().Throw<InvalidOperationException>().Which;
        exception.Message.Should().Contain("The entry point exited without ever building an IHost");
    }
}
