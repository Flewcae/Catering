using Catering.CenterService.Domain;

namespace Catering.CenterService.Application.Abstractions;

public interface ICenterRepository
{
    Task AddAsync(Center center, CancellationToken cancellationToken);
    Task<Center?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Center>> GetAllAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
