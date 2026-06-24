using Catering.BuildingBlocks.Messaging;

namespace Catering.CenterService.Application.IntegrationEvents;

public sealed record CenterCreatedIntegrationEvent(Guid CenterId, string Name, string Address) : IntegrationEvent;
