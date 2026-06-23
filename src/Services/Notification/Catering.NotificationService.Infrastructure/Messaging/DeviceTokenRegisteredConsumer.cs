using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catering.NotificationService.Infrastructure.Messaging;

public sealed class DeviceTokenRegisteredConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<KafkaConsumerBackgroundService<DeviceTokenRegisteredIntegrationEvent>> logger)
    : KafkaConsumerBackgroundService<DeviceTokenRegisteredIntegrationEvent>(scopeFactory, options, logger)
{
    protected override string Topic => KafkaTopics.DeviceTokenRegisteredEvents;
    protected override string GroupId => "catering-notification-service-device-token-registered-events";
}
