using Catering.BuildingBlocks.Messaging;

namespace Catering.UserService.Application.IntegrationEvents;

public sealed record PasswordChangedIntegrationEvent(
    Guid UserId,
    string FirstName,
    string Email) : IntegrationEvent;
