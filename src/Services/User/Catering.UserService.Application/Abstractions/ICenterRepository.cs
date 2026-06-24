using Catering.UserService.Domain;

namespace Catering.UserService.Application.Abstractions;

public interface ICenterRepository
{
    Task<Center?> GetByCenterIdAsync(Guid centerId, CancellationToken cancellationToken);
    Task AddAsync(Center center, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
