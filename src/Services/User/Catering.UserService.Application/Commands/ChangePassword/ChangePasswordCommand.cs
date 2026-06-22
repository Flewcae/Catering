using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : ICommand;
