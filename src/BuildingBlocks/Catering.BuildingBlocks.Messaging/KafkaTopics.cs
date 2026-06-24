namespace Catering.BuildingBlocks.Messaging;

public static class KafkaTopics
{
    public const string UserEvents = "catering.user-events";
    public const string NotificationEvents = "catering.notification-events";
    public const string PasswordResetRequestedEvents = "catering.password-reset-requested-events";
    public const string PasswordChangedEvents = "catering.password-changed-events";
    public const string DeviceTokenRegisteredEvents = "catering.device-token-registered-events";
    public const string DeviceTokenRevokedEvents = "catering.device-token-revoked-events";
    public const string CenterCreatedEvents = "catering.center-created-events";
}
