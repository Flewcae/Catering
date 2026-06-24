using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catering.UserService.Infrastructure.Messaging;

public sealed class CenterCreatedConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<KafkaConsumerBackgroundService<CenterCreatedIntegrationEvent>> logger)
    : KafkaConsumerBackgroundService<CenterCreatedIntegrationEvent>(scopeFactory, options, logger)
{
    protected override string Topic => KafkaTopics.CenterCreatedEvents;
    protected override string GroupId => "catering-user-service-center-created-events";
}
