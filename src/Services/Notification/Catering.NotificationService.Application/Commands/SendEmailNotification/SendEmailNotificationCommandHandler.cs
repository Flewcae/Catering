using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.IntegrationEvents;
using Catering.NotificationService.Domain;

namespace Catering.NotificationService.Application.Commands.SendEmailNotification;

public sealed class SendEmailNotificationCommandHandler(
    INotificationRepository notificationRepository,
    IEmailSender emailSender,
    IEventBus eventBus) : ICommandHandler<SendEmailNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendEmailNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = Notification.Create(request.UserId, NotificationChannel.Email, request.Recipient, request.Subject, request.Body);

        try
        {
            await emailSender.SendAsync(request.Recipient, request.Subject, request.Body, cancellationToken);
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
