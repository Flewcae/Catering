using Catering.NotificationService.Application.Commands.SendEmailNotification;
using Catering.NotificationService.Application.Commands.SendPushNotification;
using Catering.NotificationService.Application.Commands.SendPushNotificationToUser;
using Catering.NotificationService.Application.Commands.SendSmsNotification;
using Catering.NotificationService.Application.Dtos;
using Catering.NotificationService.Application.Queries.GetNotificationsForUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catering.NotificationService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("email")]
    [Authorize(Policy = "send_custom_email")]
    public async Task<ActionResult<Guid>> SendEmail(SendEmailNotificationCommand command, CancellationToken cancellationToken)
    {
        var notificationId = await mediator.Send(command, cancellationToken);
        return Ok(notificationId);
    }

    [HttpPost("sms")]
    [Authorize(Policy = "send_custom_sms")]
    public async Task<ActionResult<Guid>> SendSms(SendSmsNotificationCommand command, CancellationToken cancellationToken)
    {
        var notificationId = await mediator.Send(command, cancellationToken);
        return Ok(notificationId);
    }

    [HttpPost("push")]
    [Authorize(Policy = "send_custom_push_notification")]
    public async Task<ActionResult<Guid>> SendPush(SendPushNotificationCommand command, CancellationToken cancellationToken)
    {
        var notificationId = await mediator.Send(command, cancellationToken);
        return Ok(notificationId);
    }

    [HttpPost("push/user/{userId:guid}")]
    [Authorize(Policy = "send_custom_push_notification")]
    public async Task<ActionResult<List<Guid>>> SendPushToUser(Guid userId, SendPushNotificationToUserRequest request, CancellationToken cancellationToken)
    {
        var notificationIds = await mediator.Send(new SendPushNotificationToUserCommand(userId, request.Title, request.Body), cancellationToken);
        return Ok(notificationIds);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = "view_user_notification")]
    public async Task<ActionResult<List<NotificationDto>>> GetForUser(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await mediator.Send(new GetNotificationsForUserQuery(userId), cancellationToken);
        return Ok(notifications);
    }
}

public sealed record SendPushNotificationToUserRequest(string Title, string Body);
