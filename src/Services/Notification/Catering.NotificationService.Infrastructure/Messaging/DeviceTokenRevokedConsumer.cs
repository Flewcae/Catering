using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catering.NotificationService.Infrastructure.Messaging;

public sealed class DeviceTokenRevokedConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<KafkaConsumerBackgroundService<DeviceTokenRevokedIntegrationEvent>> logger)
    : KafkaConsumerBackgroundService<DeviceTokenRevokedIntegrationEvent>(scopeFactory, options, logger)
{
    protected override string Topic => KafkaTopics.DeviceTokenRevokedEvents;
    protected override string GroupId => "catering-notification-service-device-token-revoked-events";
}
