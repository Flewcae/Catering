using Catering.UserService.Domain;

namespace Catering.UserService.Application.Abstractions;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task AddAsync(DeviceToken deviceToken, CancellationToken cancellationToken);
    Task RemoveAsync(Guid userId, string token, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
