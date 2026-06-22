using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;
