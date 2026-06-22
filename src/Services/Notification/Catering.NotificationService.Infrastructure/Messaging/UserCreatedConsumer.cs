using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catering.NotificationService.Infrastructure.Messaging;

public sealed class UserCreatedConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<KafkaConsumerBackgroundService<UserCreatedIntegrationEvent>> logger)
    : KafkaConsumerBackgroundService<UserCreatedIntegrationEvent>(scopeFactory, options, logger)
{
    protected override string Topic => KafkaTopics.UserEvents;
    protected override string GroupId => "catering-notification-service-user-events";
}
