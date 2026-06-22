using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.CreateDepartment;

public sealed record CreateDepartmentCommand(string Name, string? Description) : ICommand<Guid>;
