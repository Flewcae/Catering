namespace Catering.BuildingBlocks.Messaging;

public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public string EventType => GetType().Name;
}
