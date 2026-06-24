using Catering.BuildingBlocks.CQRS;
using Catering.CenterService.Application.Dtos;

namespace Catering.CenterService.Application.Queries.GetCenterById;

public sealed record GetCenterByIdQuery(Guid Id) : IQuery<CenterDto?>;
