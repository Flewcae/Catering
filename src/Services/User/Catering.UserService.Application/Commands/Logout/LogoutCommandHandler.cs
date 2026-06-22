using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Common;

namespace Catering.UserService.Application.Commands.Logout;

public sealed class LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository) : ICommandHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokenRepository.GetByTokenHashAsync(TokenHasher.Hash(request.RefreshToken), cancellationToken);

        if (token is null || !token.IsActive)
        {
            return;
        }

        token.Revoke();
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
