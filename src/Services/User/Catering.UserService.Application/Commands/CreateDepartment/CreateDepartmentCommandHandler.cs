using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;

namespace Catering.UserService.Application.Commands.CreateDepartment;

public sealed class CreateDepartmentCommandHandler(IDepartmentRepository departmentRepository)
    : ICommandHandler<CreateDepartmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = Department.Create(request.Name, request.Description);

        await departmentRepository.AddAsync(department, cancellationToken);
        await departmentRepository.SaveChangesAsync(cancellationToken);

        return department.Id;
    }
}
