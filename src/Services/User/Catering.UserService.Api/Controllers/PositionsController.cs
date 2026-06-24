using Catering.UserService.Application.Commands.CreatePosition;
using Catering.UserService.Application.Commands.UpdatePositionPermissions;
using Catering.UserService.Application.Dtos;
using Catering.UserService.Application.Queries.GetPositions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catering.UserService.Api.Controllers;

[ApiController]
[Route("api/positions")]
[Authorize]
public sealed class PositionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PositionDto>>> GetPositions(CancellationToken cancellationToken)
    {
        var positions = await mediator.Send(new GetPositionsQuery(), cancellationToken);
        return Ok(positions);
    }

    [HttpPost]
    [Authorize(Policy = "manage_positions")]
    public async Task<ActionResult<Guid>> CreatePosition(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    // Intentionally role-only (no flag): assigning permission flags is the privilege-granting
    // operation itself — delegating it via a flag would let a holder grant themselves any flag.
    [HttpPut("{id:guid}/permissions")]
    [Authorize(Roles = "HRAdmin,SuperAdmin")]
    public async Task<IActionResult> UpdatePermissions(Guid id, UpdatePositionPermissionsRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdatePositionPermissionsCommand(id, request.Permissions), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdatePositionPermissionsRequest(List<string> Permissions);
