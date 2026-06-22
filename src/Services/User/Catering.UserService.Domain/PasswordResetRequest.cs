using Catering.BuildingBlocks.Domain;
using Catering.UserService.Domain.Enums;

namespace Catering.UserService.Domain;

public sealed class PasswordResetRequest : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public string CodeHash { get; private set; } = string.Empty;
    public NotificationChannelPreference Channel { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    private PasswordResetRequest()
    {
    }

    public static PasswordResetRequest Create(Guid userId, string codeHash, NotificationChannelPreference channel, DateTimeOffset expiresAt) =>
        new() { UserId = userId, CodeHash = codeHash, Channel = channel, ExpiresAt = expiresAt };

    public bool IsValid => !IsUsed && ExpiresAt > DateTimeOffset.UtcNow;

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTimeOffset.UtcNow;
        Touch();
    }
}
