using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;

namespace Catering.UserService.Application.Commands.UpdateEmploymentDetails;

public sealed class UpdateEmploymentDetailsCommandHandler(
    IUserRepository userRepository,
    IDepartmentRepository departmentRepository,
    IPositionRepository positionRepository) : ICommandHandler<UpdateEmploymentDetailsCommand>
{
    public async Task Handle(UpdateEmploymentDetailsCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User '{request.UserId}' was not found.");

        if (await departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken) is null)
        {
            throw new NotFoundException($"Department '{request.DepartmentId}' was not found.");
        }

        if (await positionRepository.GetByIdAsync(request.PositionId, cancellationToken) is null)
        {
            throw new NotFoundException($"Position '{request.PositionId}' was not found.");
        }

        user.UpdateEmploymentDetails(request.DepartmentId, request.PositionId, request.SalaryCeiling, request.HasDisability, request.DisabilityDescription, request.Notes);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
