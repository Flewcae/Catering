using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.CreatePosition;

public sealed record CreatePositionCommand(string Name, string? Description) : ICommand<Guid>;
