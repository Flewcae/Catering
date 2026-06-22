using Catering.UserService.Domain;

namespace Catering.UserService.Application.Abstractions;

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AccessTokenResult GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTimeOffset GetRefreshTokenExpiry();
}
