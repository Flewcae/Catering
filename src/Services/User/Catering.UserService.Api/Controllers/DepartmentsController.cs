using Catering.UserService.Application.Commands.CreateDepartment;
using Catering.UserService.Application.Dtos;
using Catering.UserService.Application.Queries.GetDepartments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catering.UserService.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public sealed class DepartmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetDepartments(CancellationToken cancellationToken)
    {
        var departments = await mediator.Send(new GetDepartmentsQuery(), cancellationToken);
        return Ok(departments);
    }

    [HttpPost]
    [Authorize(Policy = "manage_departments")]
    public async Task<ActionResult<Guid>> CreateDepartment(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }
}
