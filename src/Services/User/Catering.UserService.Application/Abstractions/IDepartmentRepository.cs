using Catering.UserService.Domain;

namespace Catering.UserService.Application.Abstractions;

public interface IDepartmentRepository
{
    Task AddAsync(Department department, CancellationToken cancellationToken);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Department>> GetAllAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
