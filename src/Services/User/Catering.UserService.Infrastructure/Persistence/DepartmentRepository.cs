using Catering.UserService.Application.Abstractions;
using Catering.UserService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catering.UserService.Infrastructure.Persistence;

public sealed class DepartmentRepository(UserDbContext dbContext) : IDepartmentRepository
{
    public async Task AddAsync(Department department, CancellationToken cancellationToken) =>
        await dbContext.Departments.AddAsync(department, cancellationToken);

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<List<Department>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Departments.AsNoTracking().ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
