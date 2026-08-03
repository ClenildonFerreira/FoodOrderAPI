
    using FoodOrderAPI.Application.Auth.Commands.Login;
    using FoodOrderAPI.Application.Interfaces;
    using FoodOrderAPI.Domain.Entities;
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Moq;

    namespace FoodOrderAPI.Tests.Application.Auth.Commands.Login;

    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock = new();
        private readonly IConfiguration _config;
        private readonly LoginCommandHandler _sut;

        public LoginCommandHandlerTests()
        {
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "FoodOrderAPI_SuperSecretKey_Minimo32Caracteres!",
                    ["Jwt:Issuer"] = "FoodOrderAPI",
                    ["Jwt:Audience"] = "FoodOrderAPI",
                    ["Jwt:ExpirationMinutes"] = "120"
                })
                .Build();

            _sut = new LoginCommandHandler(_userRepositoryMock.Object, _config, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new User("Admin", "admin@restaurante.com", "hash_valido", UserRole.Admin);

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync("admin@restaurante.com"))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(user, "hash_valido", "senha_certa"))
                .Returns(PasswordVerificationResult.Success);

            var command = new LoginCommand { Email = "admin@restaurante.com", Password = "senha_certa" };

            // Act
            var result = await _sut.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var command = new LoginCommand { Email = "naoexiste@restaurante.com", Password = "senha" };

            // Act
            var result = await _sut.Handle(command, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be("Email ou senha inválidos.");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenPasswordIsIncorrect()
        {
            // Arrange
            var user = new User("Admin", "admin@restaurante.com", "hash_valido", UserRole.Admin);

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync("admin@restaurante.com"))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(user, "hash_valido", "senha_errada"))
                .Returns(PasswordVerificationResult.Failed);

            var command = new LoginCommand { Email = "admin@restaurante.com", Password = "senha_errada" };

            // Act
            var result = await _sut.Handle(command, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be("Email ou senha inválidos.");
        }
    }