using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Code, string NewPassword) : ICommand;
