using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class DeviceTokenRepository(UserDbContext dbContext) : IDeviceTokenRepository
{
    public Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken cancellationToken) =>
        dbContext.DeviceTokens.FirstOrDefaultAsync(dt => dt.Token == token, cancellationToken);

    public async Task AddAsync(DeviceToken deviceToken, CancellationToken cancellationToken) =>
        await dbContext.DeviceTokens.AddAsync(deviceToken, cancellationToken);

    public async Task RemoveAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        var deviceToken = await dbContext.DeviceTokens
            .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == token, cancellationToken);

        if (deviceToken is not null)
        {
            dbContext.DeviceTokens.Remove(deviceToken);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
