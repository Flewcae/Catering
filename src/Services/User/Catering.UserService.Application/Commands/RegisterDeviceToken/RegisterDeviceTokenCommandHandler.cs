using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.IntegrationEvents;
using Catering.UserService.Domain;

namespace Catering.UserService.Application.Commands.RegisterDeviceToken;

public sealed class RegisterDeviceTokenCommandHandler(
    IDeviceTokenRepository deviceTokenRepository,
    IEventBus eventBus) : ICommandHandler<RegisterDeviceTokenCommand>
{
    public async Task Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await deviceTokenRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (existing is not null)
        {
            existing.AssignTo(request.UserId);
        }
        else
        {
            var deviceToken = DeviceToken.Create(request.UserId, request.Token, request.Platform);
            await deviceTokenRepository.AddAsync(deviceToken, cancellationToken);
        }

        await deviceTokenRepository.SaveChangesAsync(cancellationToken);

        var integrationEvent = new DeviceTokenRegisteredIntegrationEvent(
            request.UserId, request.Token, request.Platform, DateTimeOffset.UtcNow);

        await eventBus.PublishAsync(integrationEvent, KafkaTopics.DeviceTokenRegisteredEvents, cancellationToken);
    }
}
