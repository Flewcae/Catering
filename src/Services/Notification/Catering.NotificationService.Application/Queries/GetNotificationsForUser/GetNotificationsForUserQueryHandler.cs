using Catering.BuildingBlocks.CQRS;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.Dtos;

namespace Catering.NotificationService.Application.Queries.GetNotificationsForUser;

public sealed class GetNotificationsForUserQueryHandler(INotificationRepository notificationRepository)
    : IQueryHandler<GetNotificationsForUserQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetNotificationsForUserQuery request, CancellationToken cancellationToken)
    {
        var notifications = await notificationRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return notifications
            .Select(n => new NotificationDto(n.Id, n.UserId, n.Channel.ToString(), n.Recipient, n.Subject, n.Body, n.Status.ToString(), n.SentAt, n.ErrorMessage))
            .ToList();
    }
}
