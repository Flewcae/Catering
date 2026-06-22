using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Domain.Enums;

namespace Catering.UserService.Application.Commands.UpdateUserStatus;

public sealed record UpdateUserStatusCommand(Guid UserId, UserStatus NewStatus, DateOnly? TerminationDate) : ICommand;
