using Catering.UserService.Domain;

namespace Catering.UserService.Application.Abstractions;

public interface IPositionRepository
{
    Task AddAsync(Position position, CancellationToken cancellationToken);
    Task<Position?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Position>> GetAllAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
