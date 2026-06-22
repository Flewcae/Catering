using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;
using Catering.UserService.Domain.Enums;

namespace Catering.UserService.Application.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository) : ICommandHandler<UpdateUserStatusCommand>
{
    public async Task Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User '{request.UserId}' was not found.");

        user.ChangeStatus(request.NewStatus, request.TerminationDate);
        await userRepository.SaveChangesAsync(cancellationToken);

        if (request.NewStatus != UserStatus.Active)
        {
            await refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);
        }
    }
}
