using Catering.NotificationService.Application.Abstractions;
using Catering.NotificationService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.NotificationService.Infrastructure.Persistence;

public sealed class NotificationRepository(NotificationDbContext dbContext) : INotificationRepository
{
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken) =>
        await dbContext.Notifications.AddAsync(notification, cancellationToken);

    public Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Notifications.AsNoTracking().Where(n => n.UserId == userId).ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
