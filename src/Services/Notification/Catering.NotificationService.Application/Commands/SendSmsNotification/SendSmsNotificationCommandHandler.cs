using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.IntegrationEvents;
using Catering.NotificationService.Domain;

namespace Catering.NotificationService.Application.Commands.SendSmsNotification;

public sealed class SendSmsNotificationCommandHandler(
    INotificationRepository notificationRepository,
    ISmsSender smsSender,
    IEventBus eventBus) : ICommandHandler<SendSmsNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendSmsNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = Notification.Create(request.UserId, NotificationChannel.Sms, request.Recipient, subject: null, request.Body);

        try
        {
            await smsSender.SendAsync(request.Recipient, request.Body, cancellationToken);
            notification.MarkAsSent();
        }
        catch (Exception ex)
        {
            notification.MarkAsFailed(ex.Message);
        }

        await notificationRepository.AddAsync(notification, cancellationToken);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        if (notification.Status == NotificationStatus.Sent)
        {
            var integrationEvent = new NotificationSentIntegrationEvent(notification.Id, notification.UserId, notification.Channel.ToString(), notification.Recipient);
            await eventBus.PublishAsync(integrationEvent, KafkaTopics.NotificationEvents, cancellationToken);
        }

        return notification.Id;
    }
}
