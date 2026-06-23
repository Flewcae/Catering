using Catering.BuildingBlocks.Messaging;

namespace Catering.NotificationService.Application.IntegrationEvents;

public sealed record DeviceTokenRegisteredIntegrationEvent(
    Guid UserId, string Token, string Platform, DateTimeOffset RegisteredAt) : IntegrationEvent;
