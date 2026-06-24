using System.Security.Claims;
using Catering.UserService.Application.Abstractions;

namespace Catering.UserService.Api.Security;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public Guid? CenterId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue("centerId");
            return Guid.TryParse(value, out var centerId) ? centerId : null;
        }
    }
}
