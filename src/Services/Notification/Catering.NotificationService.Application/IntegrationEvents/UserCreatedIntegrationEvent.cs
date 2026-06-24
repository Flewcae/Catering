using Catering.BuildingBlocks.Messaging;

namespace Catering.NotificationService.Application.IntegrationEvents;

public sealed record UserCreatedIntegrationEvent(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Role,
    string TemporaryPassword) : IntegrationEvent;
