using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catering.BuildingBlocks.Messaging;

public sealed class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventBus> _logger;

    public KafkaEventBus(IOptions<KafkaOptions> options, ILogger<KafkaEventBus> logger)
    {
        _logger = logger;
        var config = new ProducerConfig { BootstrapServers = options.Value.BootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var payload = JsonSerializer.Serialize(@event, @event.GetType());
        var message = new Message<string, string> { Key = @event.Id.ToString(), Value = payload };

        var result = await _producer.ProduceAsync(topic, message, cancellationToken);

        _logger.LogInformation(
            "Published {EventType} ({EventId}) to topic {Topic} [partition {Partition}, offset {Offset}]",
            @event.EventType, @event.Id, topic, result.Partition.Value, result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
