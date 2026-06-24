using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.AssignUserCenter;

public sealed record AssignUserCenterCommand(Guid UserId, Guid? CenterId) : ICommand;
