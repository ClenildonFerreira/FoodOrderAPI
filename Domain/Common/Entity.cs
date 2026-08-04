namespace FoodOrderAPI.Domain.Common;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; init; }
    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;
        
        if (ReferenceEquals(this, obj))
            return true;

        return ((Entity)obj).Id == Id;
    }

    public bool Equals(Entity? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return other.Id == Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
    
    public static bool operator ==(Entity? left, Entity? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}