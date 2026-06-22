using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catering.BuildingBlocks.Messaging;

public abstract class KafkaConsumerBackgroundService<TEvent> : BackgroundService
    where TEvent : IntegrationEvent
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaConsumerBackgroundService<TEvent>> _logger;

    protected abstract string Topic { get; }
    protected abstract string GroupId { get; }

    protected KafkaConsumerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumerBackgroundService<TEvent>> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureTopicExistsAsync(stoppingToken);
        await Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private async Task EnsureTopicExistsAsync(CancellationToken cancellationToken)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _options.BootstrapServers }).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
                [new TopicSpecification { Name = Topic, NumPartitions = 1, ReplicationFactor = 1 }]);

            _logger.LogInformation("Created topic {Topic}", Topic);
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            // Topic already exists - nothing to do.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not ensure topic {Topic} exists ahead of subscribing; relying on broker auto-create", Topic);
        }
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topic);

        _logger.LogInformation("Subscribed to topic {Topic} as group {GroupId}", Topic, GroupId);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;
                try
                {
                    result = consumer.Consume(TimeSpan.FromSeconds(1));
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming from topic {Topic}", Topic);
                    Thread.Sleep(1000);
                    continue;
                }

                if (result?.Message is null)
                {
                    continue;
                }

                var @event = JsonSerializer.Deserialize<TEvent>(result.Message.Value);
                if (@event is null)
                {
                    _logger.LogWarning("Could not deserialize message from topic {Topic}, offset {Offset}", Topic, result.Offset.Value);
                    continue;
                }

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TEvent>>();
                handler.HandleAsync(@event, stoppingToken).GetAwaiter().GetResult();
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}
