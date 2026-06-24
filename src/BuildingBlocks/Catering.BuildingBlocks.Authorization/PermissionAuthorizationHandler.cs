using Microsoft.AspNetCore.Authorization;

namespace Catering.BuildingBlocks.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        if (context.User.IsInRole("SuperAdmin") || context.User.IsInRole("HRAdmin"))
        {
            context.Succeed(requirement);
        }
        else if (context.User.HasClaim(PermissionClaimTypes.Permission, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
