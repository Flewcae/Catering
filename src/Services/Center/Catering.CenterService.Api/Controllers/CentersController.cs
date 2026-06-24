using Catering.CenterService.Application.Commands.CreateCenter;
using Catering.CenterService.Application.Dtos;
using Catering.CenterService.Application.Queries.GetCenterById;
using Catering.CenterService.Application.Queries.GetCenters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catering.CenterService.Api.Controllers;

[ApiController]
[Route("api/centers")]
[Authorize]
public sealed class CentersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CenterDto>>> GetCenters(CancellationToken cancellationToken)
    {
        var centers = await mediator.Send(new GetCentersQuery(), cancellationToken);
        return Ok(centers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CenterDto>> GetCenterById(Guid id, CancellationToken cancellationToken)
    {
        var center = await mediator.Send(new GetCenterByIdQuery(id), cancellationToken);
        return center is null ? NotFound() : Ok(center);
    }

    [HttpPost]
    [Authorize(Policy = "manage_centers")]
    public async Task<ActionResult<Guid>> CreateCenter(CreateCenterCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }
}
