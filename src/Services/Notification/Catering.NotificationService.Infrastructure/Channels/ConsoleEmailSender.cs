using Catering.NotificationService.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Catering.NotificationService.Infrastructure.Channels;

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        logger.LogInformation("[Email] To: {Recipient} | Subject: {Subject} | Body: {Body}", recipient, subject, body);
        return Task.CompletedTask;
    }
}
