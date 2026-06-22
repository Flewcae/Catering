using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class RefreshTokenRepository(UserDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken) =>
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens.Include(rt => rt.User).ThenInclude(u => u.Department)
            .Include(rt => rt.User).ThenInclude(u => u.Position)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
