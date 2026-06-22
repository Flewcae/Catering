using Catering.BuildingBlocks.Domain;
using Catering.NotificationService.Domain.Events;

namespace Catering.NotificationService.Domain;

public sealed class Notification : AggregateRoot
{
    public Guid UserId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public DateTimeOffset? SentAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Notification()
    {
    }

    public static Notification Create(Guid userId, NotificationChannel channel, string recipient, string? subject, string body)
    {
        return new Notification
        {
            UserId = userId,
            Channel = channel,
            Recipient = recipient,
            Subject = subject,
            Body = body,
        };
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        Touch();
        AddDomainEvent(new NotificationSentDomainEvent(Id, UserId, Channel, Recipient));
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        Touch();
    }
}
