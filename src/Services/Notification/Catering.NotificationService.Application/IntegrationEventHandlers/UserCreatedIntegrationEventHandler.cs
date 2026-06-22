using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.Commands.SendEmailNotification;
using Catering.NotificationService.Application.IntegrationEvents;
using MediatR;

namespace Catering.NotificationService.Application.IntegrationEventHandlers;

public sealed class UserCreatedIntegrationEventHandler(ISender sender) : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    public Task HandleAsync(UserCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var subject = $"Welcome to Catering, {@event.FirstName}!";
        var body = $"Hi {@event.FirstName}, your account has been created successfully.";

        return sender.Send(new SendEmailNotificationCommand(@event.UserId, @event.Email, subject, body), cancellationToken);
    }
}
