using Catering.BuildingBlocks.CQRS;

namespace Catering.NotificationService.Application.Commands.SendEmailNotification;

public sealed record SendEmailNotificationCommand(Guid UserId, string Recipient, string Subject, string Body) : ICommand<Guid>;
