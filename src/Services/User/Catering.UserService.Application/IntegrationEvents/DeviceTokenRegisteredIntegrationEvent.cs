using Catering.BuildingBlocks.Messaging;

namespace Catering.UserService.Application.IntegrationEvents;

public sealed record DeviceTokenRegisteredIntegrationEvent(
    Guid UserId, string Token, string Platform, DateTimeOffset RegisteredAt) : IntegrationEvent;
