namespace Catering.UserService.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
