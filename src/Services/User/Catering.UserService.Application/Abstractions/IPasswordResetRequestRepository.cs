using Catering.UserService.Domain;

namespace Catering.UserService.Application.Abstractions;

public interface IPasswordResetRequestRepository
{
    Task AddAsync(PasswordResetRequest request, CancellationToken cancellationToken);
    Task<PasswordResetRequest?> GetValidRequestAsync(Guid userId, string codeHash, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
