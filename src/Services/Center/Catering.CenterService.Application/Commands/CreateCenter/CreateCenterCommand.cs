using Catering.BuildingBlocks.CQRS;

namespace Catering.CenterService.Application.Commands.CreateCenter;

public sealed record CreateCenterCommand(string Name, string Address) : ICommand<Guid>;
