using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.IntegrationEvents;
using Catering.NotificationService.Domain;

namespace Catering.NotificationService.Application.IntegrationEventHandlers;

public sealed class DeviceTokenRegisteredIntegrationEventHandler(IDeviceTokenRepository deviceTokenRepository)
    : IIntegrationEventHandler<DeviceTokenRegisteredIntegrationEvent>
{
    public async Task HandleAsync(DeviceTokenRegisteredIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var existing = await deviceTokenRepository.GetByTokenAsync(@event.Token, cancellationToken);

        if (existing is not null)
        {
            existing.AssignTo(@event.UserId, @event.Platform);
        }
        else
        {
            var deviceToken = DeviceToken.Create(@event.UserId, @event.Token, @event.Platform);
            await deviceTokenRepository.AddAsync(deviceToken, cancellationToken);
        }

        await deviceTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
