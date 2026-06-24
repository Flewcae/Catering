using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;

namespace Catering.UserService.Application.Commands.UpdatePositionPermissions;

public sealed class UpdatePositionPermissionsCommandHandler(IPositionRepository positionRepository)
    : ICommandHandler<UpdatePositionPermissionsCommand>
{
    public async Task Handle(UpdatePositionPermissionsCommand request, CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetByIdAsync(request.PositionId, cancellationToken)
            ?? throw new NotFoundException($"Position '{request.PositionId}' was not found.");

        position.UpdatePermissions(request.Permissions);

        await positionRepository.SaveChangesAsync(cancellationToken);
    }
}
