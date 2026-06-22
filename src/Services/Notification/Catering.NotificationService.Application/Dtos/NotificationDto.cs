namespace Catering.NotificationService.Application.Dtos;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Channel,
    string Recipient,
    string? Subject,
    string Body,
    string Status,
    DateTimeOffset? SentAt,
    string? ErrorMessage);
