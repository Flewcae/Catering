using Catering.NotificationService.Domain;

namespace Catering.NotificationService.Application.Abstractions;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
