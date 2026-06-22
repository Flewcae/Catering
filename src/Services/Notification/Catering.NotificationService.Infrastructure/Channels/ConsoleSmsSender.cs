using Catering.NotificationService.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Catering.NotificationService.Infrastructure.Channels;

public sealed class ConsoleSmsSender(ILogger<ConsoleSmsSender> logger) : ISmsSender
{
    public Task SendAsync(string recipient, string body, CancellationToken cancellationToken)
    {
        logger.LogInformation("[SMS] To: {Recipient} | Body: {Body}", recipient, body);
        return Task.CompletedTask;
    }
}
