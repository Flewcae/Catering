using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.RegisterDeviceToken;

public sealed record RegisterDeviceTokenCommand(Guid UserId, string Token, string Platform) : ICommand;
