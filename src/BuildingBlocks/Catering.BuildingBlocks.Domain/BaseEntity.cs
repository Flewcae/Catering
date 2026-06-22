namespace Catering.BuildingBlocks.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }

    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    public override bool Equals(object? obj) =>
        obj is BaseEntity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
