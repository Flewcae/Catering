using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Common;
using Catering.UserService.Application.Dtos;
using Catering.UserService.Application.Exceptions;
using Catering.UserService.Domain;
using Catering.UserService.Domain.Enums;

namespace Catering.UserService.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : ICommandHandler<LoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AuthenticationException("Invalid email or password.");

        if (user.IsLockedOut())
        {
            throw new AuthenticationException("Account is temporarily locked due to too many failed login attempts.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new AuthenticationException("Account is not active.");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordLoginFailure();
            await userRepository.SaveChangesAsync(cancellationToken);
            throw new AuthenticationException("Invalid email or password.");
        }

        user.RecordLoginSuccess();

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, TokenHasher.Hash(refreshTokenValue), tokenService.GetRefreshTokenExpiry());

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(accessToken.Token, accessToken.ExpiresAt, refreshTokenValue, user.ToDto());
    }
}
