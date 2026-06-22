using Catering.BuildingBlocks.CQRS;

namespace Catering.NotificationService.Application.Commands.SendPushNotification;

public sealed record SendPushNotificationCommand(Guid UserId, string DeviceToken, string Title, string Body) : ICommand<Guid>;
