using FoodOrderAPI.Application.Auth.Commands.Register;
using FoodOrderAPI.Application.DTOs;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Domain.Common;
using FoodOrderAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public RegisterUserHandler(
        IUserRepository userRepository, 
        IPasswordHasher<User> passwordHasher
        )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(command.Email);
        if (existingUser is not null)
            return Result.Failure<UserDto> ("Já existe um usuário cadastrado com este e-mail.");

        if (!Enum.IsDefined(typeof(UserRole), command.Role))
            return Result.Failure<UserDto>("Função de usuário inválida.");

        var user = new User(command.Name, command.Email, "temp", (UserRole)command.Role);

        var hashedPassword = _passwordHasher.HashPassword(user, command.Password);
        
        user.UpdatePasswordHash(hashedPassword);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return Result.Success(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive
        });
    }

}