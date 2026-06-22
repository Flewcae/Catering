using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Queries.GetPositions;

public sealed class GetPositionsQueryHandler(IPositionRepository positionRepository)
    : IQueryHandler<GetPositionsQuery, List<PositionDto>>
{
    public async Task<List<PositionDto>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
    {
        var positions = await positionRepository.GetAllAsync(cancellationToken);

        return positions.Select(p => new PositionDto(p.Id, p.Name, p.Description)).ToList();
    }
}
