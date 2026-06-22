using Catering.NotificationService.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Catering.NotificationService.Infrastructure.Channels;

public sealed class ConsolePushNotificationSender(ILogger<ConsolePushNotificationSender> logger) : IPushNotificationSender
{
    public Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken)
    {
        logger.LogInformation("[Push] Device: {DeviceToken} | Title: {Title} | Body: {Body}", deviceToken, title, body);
        return Task.CompletedTask;
    }
}
