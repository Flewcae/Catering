using Catering.CenterService.Application.Abstractions;
using Catering.CenterService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.CenterService.Infrastructure.Persistence;

public sealed class CenterRepository(CenterDbContext dbContext) : ICenterRepository
{
    public async Task AddAsync(Center center, CancellationToken cancellationToken) =>
        await dbContext.Centers.AddAsync(center, cancellationToken);

    public Task<Center?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Centers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<Center>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Centers.AsNoTracking().ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
