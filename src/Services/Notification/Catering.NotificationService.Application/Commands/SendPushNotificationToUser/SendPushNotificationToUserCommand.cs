using Catering.BuildingBlocks.CQRS;

namespace Catering.NotificationService.Application.Commands.SendPushNotificationToUser;

public sealed record SendPushNotificationToUserCommand(Guid UserId, string Title, string Body) : ICommand<List<Guid>>;
