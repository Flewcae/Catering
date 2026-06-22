using Catering.BuildingBlocks.Messaging;

namespace Catering.NotificationService.Application.IntegrationEvents;

public sealed record NotificationSentIntegrationEvent(
    Guid NotificationId,
    Guid UserId,
    string Channel,
    string Recipient) : IntegrationEvent;
