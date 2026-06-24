using Catering.BuildingBlocks.CQRS;
using Catering.CenterService.Application.Dtos;

namespace Catering.CenterService.Application.Queries.GetCenters;

public sealed record GetCentersQuery : IQuery<List<CenterDto>>;
