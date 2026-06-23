using Catering.NotificationService.Domain;

namespace Catering.NotificationService.Application.Abstractions;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task<List<DeviceToken>> GetTokensForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(DeviceToken deviceToken, CancellationToken cancellationToken);
    Task RemoveAsync(Guid userId, string token, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
