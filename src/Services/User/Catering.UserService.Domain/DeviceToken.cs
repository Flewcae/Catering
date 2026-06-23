using Catering.BuildingBlocks.Domain;

namespace Catering.UserService.Domain;

public sealed class DeviceToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public string Token { get; private set; } = string.Empty;
    public string Platform { get; private set; } = string.Empty;

    private DeviceToken()
    {
    }

    public static DeviceToken Create(Guid userId, string token, string platform) =>
        new() { UserId = userId, Token = token, Platform = platform };

    public void AssignTo(Guid userId)
    {
        UserId = userId;
        Touch();
    }
}
