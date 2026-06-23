using Catering.BuildingBlocks.Messaging;

namespace Catering.NotificationService.Application.IntegrationEvents;

public sealed record DeviceTokenRevokedIntegrationEvent(Guid UserId, string Token) : IntegrationEvent;
