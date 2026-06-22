using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResultDto>;
