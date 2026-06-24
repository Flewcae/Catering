using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class CenterRepository(UserDbContext dbContext) : ICenterRepository
{
    public Task<Center?> GetByCenterIdAsync(Guid centerId, CancellationToken cancellationToken) =>
        dbContext.Centers.FirstOrDefaultAsync(c => c.CenterId == centerId, cancellationToken);

    public async Task AddAsync(Center center, CancellationToken cancellationToken) =>
        await dbContext.Centers.AddAsync(center, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
