using FoodOrderAPI.Domain.Entities;

namespace FoodOrderAPI.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task SaveChangesAsync();
}