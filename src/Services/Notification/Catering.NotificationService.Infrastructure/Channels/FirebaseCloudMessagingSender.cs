using Catering.NotificationService.Application.Abstractions;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catering.NotificationService.Infrastructure.Channels;

public sealed class FirebaseCloudMessagingSender(IOptions<FirebaseOptions> options, ILogger<FirebaseCloudMessagingSender> logger) : IPushNotificationSender
{
    private static readonly object InitLock = new();

    private readonly FirebaseOptions _options = options.Value;

    public async Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken)
    {
        EnsureInitialized();

        var message = new Message
        {
            Token = deviceToken,
            Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
        };

        var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);

        logger.LogInformation("[Push] Sent to {DeviceToken} via Firebase ({MessageId})", deviceToken, messageId);
    }

    private void EnsureInitialized()
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        lock (InitLock)
        {
            if (FirebaseApp.DefaultInstance is not null)
            {
                return;
            }

            FirebaseApp.Create(new AppOptions
            {
                ProjectId = _options.ProjectId,
                Credential = CredentialFactory.FromJson<ServiceAccountCredential>(_options.CredentialsJson).ToGoogleCredential(),
            });
        }
    }
}
