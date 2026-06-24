using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;

namespace Catering.UserService.Application.Commands.AssignUserCenter;

public sealed class AssignUserCenterCommandHandler(
    IUserRepository userRepository,
    ICenterRepository centerRepository) : ICommandHandler<AssignUserCenterCommand>
{
    public async Task Handle(AssignUserCenterCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User '{request.UserId}' was not found.");

        if (request.CenterId.HasValue && await centerRepository.GetByCenterIdAsync(request.CenterId.Value, cancellationToken) is null)
        {
            throw new NotFoundException($"Center '{request.CenterId}' was not found.");
        }

        user.AssignCenter(request.CenterId);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
