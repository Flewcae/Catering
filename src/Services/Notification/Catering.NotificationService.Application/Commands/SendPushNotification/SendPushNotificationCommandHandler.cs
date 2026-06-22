using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.IntegrationEvents;
using Catering.NotificationService.Domain;

namespace Catering.NotificationService.Application.Commands.SendPushNotification;

public sealed class SendPushNotificationCommandHandler(
    INotificationRepository notificationRepository,
    IPushNotificationSender pushNotificationSender,
    IEventBus eventBus) : ICommandHandler<SendPushNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendPushNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = Notification.Create(request.UserId, NotificationChannel.Push, request.DeviceToken, request.Title, request.Body);

        try
        {
            await pushNotificationSender.SendAsync(request.DeviceToken, request.Title, request.Body, cancellationToken);
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
