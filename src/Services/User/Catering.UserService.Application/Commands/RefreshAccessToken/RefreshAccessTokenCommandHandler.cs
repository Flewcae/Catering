using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Common;
using Catering.UserService.Application.Dtos;
using Catering.UserService.Application.Exceptions;
using Catering.UserService.Domain;

namespace Catering.UserService.Application.Commands.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService) : ICommandHandler<RefreshAccessTokenCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(TokenHasher.Hash(request.RefreshToken), cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            throw new AuthenticationException("Refresh token is invalid or expired.");
        }

        existingToken.Revoke();

        var user = existingToken.User;
        var accessToken = tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(user.Id, TokenHasher.Hash(newRefreshTokenValue), tokenService.GetRefreshTokenExpiry());

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(accessToken.Token, accessToken.ExpiresAt, newRefreshTokenValue, user.ToDto());
    }
}
