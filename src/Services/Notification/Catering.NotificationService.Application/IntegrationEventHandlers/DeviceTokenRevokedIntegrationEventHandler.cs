using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.IntegrationEvents;

namespace Catering.NotificationService.Application.IntegrationEventHandlers;

public sealed class DeviceTokenRevokedIntegrationEventHandler(IDeviceTokenRepository deviceTokenRepository)
    : IIntegrationEventHandler<DeviceTokenRevokedIntegrationEvent>
{
    public async Task HandleAsync(DeviceTokenRevokedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        await deviceTokenRepository.RemoveAsync(@event.UserId, @event.Token, cancellationToken);
        await deviceTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
