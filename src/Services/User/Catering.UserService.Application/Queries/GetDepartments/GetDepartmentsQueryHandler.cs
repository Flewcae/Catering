using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Queries.GetDepartments;

public sealed class GetDepartmentsQueryHandler(IDepartmentRepository departmentRepository)
    : IQueryHandler<GetDepartmentsQuery, List<DepartmentDto>>
{
    public async Task<List<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.GetAllAsync(cancellationToken);

        return departments.Select(d => new DepartmentDto(d.Id, d.Name, d.Description)).ToList();
    }
}
