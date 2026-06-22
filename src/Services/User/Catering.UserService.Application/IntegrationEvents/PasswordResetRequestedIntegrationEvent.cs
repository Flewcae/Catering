using Catering.BuildingBlocks.Messaging;

namespace Catering.UserService.Application.IntegrationEvents;

public sealed record PasswordResetRequestedIntegrationEvent(
    Guid UserId,
    string FirstName,
    string Email,
    string PhoneNumber,
    string Code,
    string Channel,
    DateTimeOffset ExpiresAt) : IntegrationEvent;
