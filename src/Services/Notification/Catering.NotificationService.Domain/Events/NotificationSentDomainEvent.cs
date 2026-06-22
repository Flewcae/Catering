using Catering.BuildingBlocks.Domain;

namespace Catering.NotificationService.Domain.Events;

public sealed record NotificationSentDomainEvent(
    Guid NotificationId,
    Guid UserId,
    NotificationChannel Channel,
    string Recipient) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
