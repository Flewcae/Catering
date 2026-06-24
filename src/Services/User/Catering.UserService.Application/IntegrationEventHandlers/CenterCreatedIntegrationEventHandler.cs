using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.IntegrationEvents;
using Catering.UserService.Domain;

namespace Catering.UserService.Application.IntegrationEventHandlers;

public sealed class CenterCreatedIntegrationEventHandler(ICenterRepository centerRepository)
    : IIntegrationEventHandler<CenterCreatedIntegrationEvent>
{
    public async Task HandleAsync(CenterCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var existing = await centerRepository.GetByCenterIdAsync(@event.CenterId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateFrom(@event.Name, @event.Address);
        }
        else
        {
            var center = Center.Create(@event.CenterId, @event.Name, @event.Address);
            await centerRepository.AddAsync(center, cancellationToken);
        }

        await centerRepository.SaveChangesAsync(cancellationToken);
    }
}
