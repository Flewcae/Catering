using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.IntegrationEvents;

namespace Catering.UserService.Application.Commands.RevokeDeviceToken;

public sealed class RevokeDeviceTokenCommandHandler(
    IDeviceTokenRepository deviceTokenRepository,
    IEventBus eventBus) : ICommandHandler<RevokeDeviceTokenCommand>
{
    public async Task Handle(RevokeDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        await deviceTokenRepository.RemoveAsync(request.UserId, request.Token, cancellationToken);
        await deviceTokenRepository.SaveChangesAsync(cancellationToken);

        var integrationEvent = new DeviceTokenRevokedIntegrationEvent(request.UserId, request.Token);
        await eventBus.PublishAsync(integrationEvent, KafkaTopics.DeviceTokenRevokedEvents, cancellationToken);
    }
}
