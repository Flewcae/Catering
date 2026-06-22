using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Commands.RefreshAccessToken;

public sealed record RefreshAccessTokenCommand(string RefreshToken) : ICommand<AuthResultDto>;
