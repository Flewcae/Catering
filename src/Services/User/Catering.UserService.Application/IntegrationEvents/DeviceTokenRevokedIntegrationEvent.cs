using Catering.BuildingBlocks.Messaging;

namespace Catering.UserService.Application.IntegrationEvents;

public sealed record DeviceTokenRevokedIntegrationEvent(Guid UserId, string Token) : IntegrationEvent;
