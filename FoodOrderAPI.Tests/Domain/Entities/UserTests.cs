using FoodOrderAPI.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace FoodOrderAPI.Tests.Domain.Entities
{
    public class UserTests
    {
        [Fact]
        public void Create_ValidUser_ShouldCreateUserSuccessfully()
        {
            // Arrange & Act
            var user = new User("John Doe", "john@example.com", "hash123", UserRole.Waiter);

            // Assert
            user.Should().NotBeNull();
            user.Name.Should().Be("John Doe");
            user.Email.Should().Be("john@example.com");
            user.PasswordHash.Should().Be("hash123");
            user.Role.Should().Be(UserRole.Waiter);
            user.IsActive.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_InvalidName_ShouldThrowArgumentException(string? invalidName)
        {
            // Arrange
            Action act = () => new User(invalidName, "john@example.com", "hash123", UserRole.Waiter);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Name*");
        }

        [Fact]
        public void Create_InvalidEmail_ShouldThrowArgumentException()
        {
            // Arrange
            Action act = () => new User("John", "", "hash123", UserRole.Waiter);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Email*");
        }

        [Fact]
        public void ChangeRole_ToAdmin_ShouldUpdateRole()
        {
            // Arrange
            var user = new User("John Doe", "john@example.com", "hash123", UserRole.Waiter);

            // Act
            user.ChangeRole(UserRole.Admin);

            // Assert
            user.Role.Should().Be(UserRole.Admin);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var user = new User("John Doe", "john@example.com", "hash123", UserRole.Waiter);

            // Act
            user.Deactivate();

            // Assert
            user.IsActive.Should().BeFalse();
        }
    }
}
