using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.CenterService.Application.Abstractions;
using Catering.CenterService.Application.IntegrationEvents;
using Catering.CenterService.Domain;

namespace Catering.CenterService.Application.Commands.CreateCenter;

public sealed class CreateCenterCommandHandler(ICenterRepository centerRepository, IEventBus eventBus)
    : ICommandHandler<CreateCenterCommand, Guid>
{
    public async Task<Guid> Handle(CreateCenterCommand request, CancellationToken cancellationToken)
    {
        var center = Center.Create(request.Name, request.Address);

        await centerRepository.AddAsync(center, cancellationToken);
        await centerRepository.SaveChangesAsync(cancellationToken);

        var integrationEvent = new CenterCreatedIntegrationEvent(center.Id, center.Name, center.Address);
        await eventBus.PublishAsync(integrationEvent, KafkaTopics.CenterCreatedEvents, cancellationToken);

        return center.Id;
    }
}
