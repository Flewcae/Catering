using System.Security.Cryptography;
using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Common;
using Catering.UserService.Application.IntegrationEvents;
using Catering.UserService.Domain;

namespace Catering.UserService.Application.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IPasswordResetRequestRepository passwordResetRequestRepository,
    IEventBus eventBus) : ICommandHandler<RequestPasswordResetCommand>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return;
        }

        var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var expiresAt = DateTimeOffset.UtcNow.Add(CodeLifetime);

        var resetRequest = PasswordResetRequest.Create(user.Id, TokenHasher.Hash(code), request.Channel, expiresAt);

        await passwordResetRequestRepository.AddAsync(resetRequest, cancellationToken);
        await passwordResetRequestRepository.SaveChangesAsync(cancellationToken);

        var integrationEvent = new PasswordResetRequestedIntegrationEvent(
            user.Id, user.FirstName, user.Email, user.PhoneNumber, code, request.Channel.ToString(), expiresAt);

        await eventBus.PublishAsync(integrationEvent, KafkaTopics.PasswordResetRequestedEvents, cancellationToken);
    }
}
