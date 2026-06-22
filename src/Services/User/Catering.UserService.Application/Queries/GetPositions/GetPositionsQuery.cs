using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Queries.GetPositions;

public sealed record GetPositionsQuery : IQuery<List<PositionDto>>;
