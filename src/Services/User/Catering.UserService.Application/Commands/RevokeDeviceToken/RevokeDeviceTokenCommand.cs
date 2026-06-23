using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.RevokeDeviceToken;

public sealed record RevokeDeviceTokenCommand(Guid UserId, string Token) : ICommand;
