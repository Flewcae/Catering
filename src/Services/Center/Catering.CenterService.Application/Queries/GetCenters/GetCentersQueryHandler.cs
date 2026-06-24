using Catering.BuildingBlocks.CQRS;
using Catering.CenterService.Application.Abstractions;
using Catering.CenterService.Application.Dtos;

namespace Catering.CenterService.Application.Queries.GetCenters;

public sealed class GetCentersQueryHandler(ICenterRepository centerRepository)
    : IQueryHandler<GetCentersQuery, List<CenterDto>>
{
    public async Task<List<CenterDto>> Handle(GetCentersQuery request, CancellationToken cancellationToken)
    {
        var centers = await centerRepository.GetAllAsync(cancellationToken);

        return centers.Select(c => new CenterDto(c.Id, c.Name, c.Address)).ToList();
    }
}
