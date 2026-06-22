using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Common;
using Catering.UserService.Application.Exceptions;
using Catering.UserService.Application.IntegrationEvents;

namespace Catering.UserService.Application.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetRequestRepository passwordResetRequestRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IEventBus eventBus) : ICommandHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AuthenticationException("Invalid or expired reset code.");

        var resetRequest = await passwordResetRequestRepository.GetValidRequestAsync(user.Id, TokenHasher.Hash(request.Code), cancellationToken)
            ?? throw new AuthenticationException("Invalid or expired reset code.");

        resetRequest.MarkAsUsed();
        user.ChangePassword(passwordHasher.Hash(request.NewPassword));

        await userRepository.SaveChangesAsync(cancellationToken);
        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        var integrationEvent = new PasswordChangedIntegrationEvent(user.Id, user.FirstName, user.Email);
        await eventBus.PublishAsync(integrationEvent, KafkaTopics.PasswordChangedEvents, cancellationToken);
    }
}
