using Microsoft.AspNetCore.Authorization;

namespace Catering.BuildingBlocks.Authorization;

public sealed class PermissionAuthorizationRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
