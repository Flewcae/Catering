using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class PositionRepository(UserDbContext dbContext) : IPositionRepository
{
    public async Task AddAsync(Position position, CancellationToken cancellationToken) =>
        await dbContext.Positions.AddAsync(position, cancellationToken);

    public Task<Position?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Positions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<Position>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Positions.AsNoTracking().ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
