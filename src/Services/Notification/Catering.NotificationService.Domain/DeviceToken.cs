using Catering.BuildingBlocks.Domain;

namespace Catering.NotificationService.Domain;

public sealed class DeviceToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string Platform { get; private set; } = string.Empty;

    private DeviceToken()
    {
    }

    public static DeviceToken Create(Guid userId, string token, string platform) =>
        new() { UserId = userId, Token = token, Platform = platform };

    public void AssignTo(Guid userId, string platform)
    {
        UserId = userId;
        Platform = platform;
        Touch();
    }
}
