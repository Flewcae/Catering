namespace Catering.NotificationService.Application.Abstractions;

public interface ISmsSender
{
    Task SendAsync(string recipient, string body, CancellationToken cancellationToken);
}
