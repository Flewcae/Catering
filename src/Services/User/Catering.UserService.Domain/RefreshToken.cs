using Catering.BuildingBlocks.Domain;

namespace Catering.UserService.Domain;

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAt) =>
        new() { UserId = userId, TokenHash = tokenHash, ExpiresAt = expiresAt };

    public bool IsActive => !IsRevoked && ExpiresAt > DateTimeOffset.UtcNow;

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        Touch();
    }
}
