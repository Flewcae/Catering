using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Queries.GetDepartments;

public sealed record GetDepartmentsQuery : IQuery<List<DepartmentDto>>;
