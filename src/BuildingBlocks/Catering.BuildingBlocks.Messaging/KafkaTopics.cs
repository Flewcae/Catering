namespace Catering.BuildingBlocks.Messaging;

public static class KafkaTopics
{
    public const string UserEvents = "catering.user-events";
    public const string NotificationEvents = "catering.notification-events";
    public const string PasswordResetRequestedEvents = "catering.password-reset-requested-events";
    public const string PasswordChangedEvents = "catering.password-changed-events";
}
