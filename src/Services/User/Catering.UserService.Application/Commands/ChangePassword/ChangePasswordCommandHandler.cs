using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;
using Catering.UserService.Application.IntegrationEvents;

namespace Catering.UserService.Application.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IEventBus eventBus) : ICommandHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User '{request.UserId}' was not found.");

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new AuthenticationException("Current password is incorrect.");
        }

        user.ChangePassword(passwordHasher.Hash(request.NewPassword));
        await userRepository.SaveChangesAsync(cancellationToken);

        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        var integrationEvent = new PasswordChangedIntegrationEvent(user.Id, user.FirstName, user.Email);
        await eventBus.PublishAsync(integrationEvent, KafkaTopics.PasswordChangedEvents, cancellationToken);
    }
}
