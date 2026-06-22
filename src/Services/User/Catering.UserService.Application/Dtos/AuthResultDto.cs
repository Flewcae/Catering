namespace Catering.UserService.Application.Dtos;

public sealed record AuthResultDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    UserDto User);
