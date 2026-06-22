using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class PasswordResetRequestRepository(UserDbContext dbContext) : IPasswordResetRequestRepository
{
    public async Task AddAsync(PasswordResetRequest request, CancellationToken cancellationToken) =>
        await dbContext.PasswordResetRequests.AddAsync(request, cancellationToken);

    public Task<PasswordResetRequest?> GetValidRequestAsync(Guid userId, string codeHash, CancellationToken cancellationToken) =>
        dbContext.PasswordResetRequests
            .Where(p => p.UserId == userId && p.CodeHash == codeHash && !p.IsUsed && p.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
