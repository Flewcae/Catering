using Catering.BuildingBlocks.CQRS;
using Catering.CenterService.Application.Abstractions;
using Catering.CenterService.Application.Dtos;

namespace Catering.CenterService.Application.Queries.GetCenterById;

public sealed class GetCenterByIdQueryHandler(ICenterRepository centerRepository)
    : IQueryHandler<GetCenterByIdQuery, CenterDto?>
{
    public async Task<CenterDto?> Handle(GetCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var center = await centerRepository.GetByIdAsync(request.Id, cancellationToken);

        return center is null ? null : new CenterDto(center.Id, center.Name, center.Address);
    }
}
