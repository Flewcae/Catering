namespace Catering.NotificationService.Application.Abstractions;

public interface IPushNotificationSender
{
    Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken);
}
