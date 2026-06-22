namespace Catering.BuildingBlocks.Messaging;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
