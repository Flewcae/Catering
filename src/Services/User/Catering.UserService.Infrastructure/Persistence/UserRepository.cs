using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await dbContext.Users.AddAsync(user, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.Include(u => u.Department).Include(u => u.Position).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.Include(u => u.Department).Include(u => u.Position).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByTcIdentityNumberAsync(string tcIdentityNumber, CancellationToken cancellationToken) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.TcIdentityNumber == tcIdentityNumber, cancellationToken);

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Users.AsNoTracking().Include(u => u.Department).Include(u => u.Position).ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
