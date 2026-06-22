using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Domain.Enums;

namespace Catering.UserService.Application.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email, NotificationChannelPreference Channel) : ICommand;
