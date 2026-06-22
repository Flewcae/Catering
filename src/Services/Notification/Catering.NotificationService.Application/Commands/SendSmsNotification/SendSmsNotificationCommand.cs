using Catering.BuildingBlocks.CQRS;

namespace Catering.NotificationService.Application.Commands.SendSmsNotification;

public sealed record SendSmsNotificationCommand(Guid UserId, string Recipient, string Body) : ICommand<Guid>;
