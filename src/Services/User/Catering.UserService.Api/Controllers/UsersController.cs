using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Commands.AssignUserCenter;
using Catering.UserService.Application.Commands.ChangePassword;
using Catering.UserService.Application.Commands.CreateUserAccount;
using Catering.UserService.Application.Commands.DeleteUser;
using Catering.UserService.Application.Commands.RegisterDeviceToken;
using Catering.UserService.Application.Commands.RevokeDeviceToken;
using Catering.UserService.Application.Commands.UpdateEmploymentDetails;
using Catering.UserService.Application.Commands.UpdateProfile;
using Catering.UserService.Application.Commands.UpdateUserStatus;
using Catering.UserService.Application.Dtos;
using Catering.UserService.Application.Queries.GetUserById;
using Catering.UserService.Application.Queries.GetUsers;
using Catering.UserService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catering.UserService.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByIdQuery(currentUserService.UserId!.Value), cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("me")]
    [Authorize(Policy = "update_self_profile")]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateProfileCommand(currentUserService.UserId!.Value, request.FirstName, request.LastName, request.PhoneNumber, request.Address, request.BirthDate, request.ProfilePictureUrl),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangeMyPassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new ChangePasswordCommand(currentUserService.UserId!.Value, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("me/device-tokens")]
    public async Task<IActionResult> RegisterDeviceToken(RegisterDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RegisterDeviceTokenCommand(currentUserService.UserId!.Value, request.Token, request.Platform),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("me/device-tokens/revoke")]
    public async Task<IActionResult> RevokeDeviceToken(RevokeDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RevokeDeviceTokenCommand(currentUserService.UserId!.Value, request.Token),
            cancellationToken);

        return NoContent();
    }

    [HttpGet]
    [Authorize(Policy = "view_users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "view_users")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = "create_account")]
    public async Task<ActionResult<Guid>> CreateUserAccount(CreateUserAccountCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "delete_user")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/profile")]
    [Authorize(Policy = "update_user_profile")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateProfileCommand(id, request.FirstName, request.LastName, request.PhoneNumber, request.Address, request.BirthDate, request.ProfilePictureUrl),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/employment-details")]
    [Authorize(Policy = "update_employment_details")]
    public async Task<IActionResult> UpdateEmploymentDetails(Guid id, UpdateEmploymentDetailsRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateEmploymentDetailsCommand(id, request.DepartmentId, request.PositionId, request.SalaryCeiling, request.HasDisability, request.DisabilityDescription, request.Notes),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "update_user_status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateUserStatusCommand(id, request.NewStatus, request.TerminationDate), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/center")]
    [Authorize(Policy = "assign_user_center")]
    public async Task<IActionResult> AssignCenter(Guid id, AssignUserCenterRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignUserCenterCommand(id, request.CenterId), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateProfileRequest(string FirstName, string LastName, string PhoneNumber, string? Address, DateOnly? BirthDate, string? ProfilePictureUrl);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record UpdateEmploymentDetailsRequest(Guid DepartmentId, Guid PositionId, decimal? SalaryCeiling, bool HasDisability, string? DisabilityDescription, string? Notes);

public sealed record UpdateUserStatusRequest(UserStatus NewStatus, DateOnly? TerminationDate);

public sealed record RegisterDeviceTokenRequest(string Token, string Platform);

public sealed record RevokeDeviceTokenRequest(string Token);

public sealed record AssignUserCenterRequest(Guid? CenterId);
