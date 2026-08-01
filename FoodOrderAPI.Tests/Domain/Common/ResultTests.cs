using FoodOrderAPI.Domain.Common;
using FluentAssertions;

namespace FoodOrderAPI.Tests.Domain.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldHaveValue_WhenGeneric()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldExposeErrorMessage()
    {
        var result = Result.Failure("erro de negócio");

        result.IsFailure.Should().BeTrue();
        result.IsNotFound.Should().BeFalse();
        result.Error.Should().Be("erro de negócio");
    }

    [Fact]
    public void NotFound_ShouldMarkIsNotFound()
    {
        var result = Result.NotFound<string>("não encontrado");

        result.IsFailure.Should().BeTrue();
        result.IsNotFound.Should().BeTrue();
        result.Error.Should().Be("não encontrado");
    }
}
