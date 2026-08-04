using FoodOrderAPI.Domain.Common;
using FoodOrderAPI.Domain.ValueObjects;

namespace FoodOrderAPI.Domain.Entities
{
    public class Product : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Money Price { get; set; } = Money.Zero();
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? ExternalId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}