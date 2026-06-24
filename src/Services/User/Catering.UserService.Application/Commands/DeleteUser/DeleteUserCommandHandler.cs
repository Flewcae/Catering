using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;

namespace Catering.UserService.Application.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler(IUserRepository userRepository) : ICommandHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User '{request.UserId}' was not found.");

        await userRepository.DeleteAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
