using Catering.BuildingBlocks.CQRS;
using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Application.Commands.SendPushNotification;
using MediatR;

namespace Catering.NotificationService.Application.Commands.SendPushNotificationToUser;

public sealed class SendPushNotificationToUserCommandHandler(
    IDeviceTokenRepository deviceTokenRepository,
    ISender sender) : ICommandHandler<SendPushNotificationToUserCommand, List<Guid>>
{
    public async Task<List<Guid>> Handle(SendPushNotificationToUserCommand request, CancellationToken cancellationToken)
    {
        var tokens = await deviceTokenRepository.GetTokensForUserAsync(request.UserId, cancellationToken);
        var notificationIds = new List<Guid>();

        foreach (var deviceToken in tokens)
        {
            var notificationId = await sender.Send(
                new SendPushNotificationCommand(request.UserId, deviceToken.Token, request.Title, request.Body),
                cancellationToken);

            notificationIds.Add(notificationId);
        }

        return notificationIds;
    }
}
