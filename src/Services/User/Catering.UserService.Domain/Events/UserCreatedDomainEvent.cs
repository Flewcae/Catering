using Catering.BuildingBlocks.Domain;
using Catering.UserService.Domain.Enums;

namespace Catering.UserService.Domain.Events;

public sealed record UserCreatedDomainEvent(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    SystemRole Role) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
