using FoodOrderAPI.Domain.Common;

namespace FoodOrderAPI.Domain.Entities
{
    public class User : AggregateRoot
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }

        public User(string name, string email, string passwordHash, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("PasswordHash cannot be empty", nameof(passwordHash));

            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
        }

        protected User() 
        { 
            Name = null!;
            Email = null!;
            PasswordHash = null!;
        } 

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void UpdatePasswordHash(string newHash)
        {
            if (string.IsNullOrWhiteSpace(newHash))
                throw new ArgumentException("Nova hash de senha não pode ser vazio", nameof(newHash));

            PasswordHash = newHash;
        }
    }
}