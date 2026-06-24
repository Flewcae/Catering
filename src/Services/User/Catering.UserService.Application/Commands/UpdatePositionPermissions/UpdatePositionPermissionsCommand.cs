using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.UpdatePositionPermissions;

public sealed record UpdatePositionPermissionsCommand(Guid PositionId, List<string> Permissions) : ICommand;
