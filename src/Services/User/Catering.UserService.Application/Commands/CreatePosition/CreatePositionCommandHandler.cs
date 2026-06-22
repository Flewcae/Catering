using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;

namespace Catering.UserService.Application.Commands.CreatePosition;

public sealed class CreatePositionCommandHandler(IPositionRepository positionRepository)
    : ICommandHandler<CreatePositionCommand, Guid>
{
    public async Task<Guid> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = Position.Create(request.Name, request.Description);

        await positionRepository.AddAsync(position, cancellationToken);
        await positionRepository.SaveChangesAsync(cancellationToken);

        return position.Id;
    }
}
