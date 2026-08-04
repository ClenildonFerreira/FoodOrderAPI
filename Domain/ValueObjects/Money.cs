using FoodOrderAPI.Domain.Common;

namespace FoodOrderAPI.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    
    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("O valor monetário não pode ser negativo.", nameof(amount));
        }

        Amount = amount;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount);
    public static Money Zero() => new(0);
    
    public static implicit operator decimal(Money money) => money.Amount;
    public static implicit operator Money(decimal amount) => new(amount);
    
}