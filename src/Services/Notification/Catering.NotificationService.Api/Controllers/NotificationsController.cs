using Catering.NotificationService.Application.Commands.SendEmailNotification;
using Catering.NotificationService.Application.Commands.SendPushNotification;
using Catering.NotificationService.Application.Commands.SendSmsNotification;
using Catering.NotificationService.Application.Dtos;
using Catering.NotificationService.Application.Queries.GetNotificationsForUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catering.NotificationService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("email")]
    public async Task<ActionResult<Guid>> SendEmail(SendEmailNotificationCommand command, CancellationToken cancellationToken)
    {
        var notificationId = await mediator.Send(command, cancellationToken);
        return Ok(notificationId);
    }

    [HttpPost("sms")]
    public async Task<ActionResult<Guid>> SendSms(SendSmsNotificationCommand command, CancellationToken cancellationToken)
    {
        var notificationId = await mediator.Send(command, cancellationToken);
        return Ok(notificationId);
    }

    [HttpPost("push")]
    public async Task<ActionResult<Guid>> SendPush(SendPushNotificationCommand command, CancellationToken cancellationToken)
    {
        var notificationId = await mediator.Send(command, cancellationToken);
        return Ok(notificationId);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<NotificationDto>>> GetForUser(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await mediator.Send(new GetNotificationsForUserQuery(userId), cancellationToken);
        return Ok(notifications);
    }
}
