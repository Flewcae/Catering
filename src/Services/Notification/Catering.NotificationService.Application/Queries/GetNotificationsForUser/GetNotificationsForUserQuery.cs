using Catering.BuildingBlocks.CQRS;
using Catering.NotificationService.Application.Dtos;

namespace Catering.NotificationService.Application.Queries.GetNotificationsForUser;

public sealed record GetNotificationsForUserQuery(Guid UserId) : IQuery<List<NotificationDto>>;
