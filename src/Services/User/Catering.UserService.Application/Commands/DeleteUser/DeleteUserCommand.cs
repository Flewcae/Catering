using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;
