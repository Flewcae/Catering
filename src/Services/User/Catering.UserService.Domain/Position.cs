using System.Text.RegularExpressions;
using Catering.BuildingBlocks.Domain;

namespace Catering.UserService.Domain;

public sealed class Position : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public List<string> Permissions { get; private set; } = [];

    private Position()
    {
    }

    public static Position Create(string name, string? description) =>
        new() { Name = name, Description = description };

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        Touch();
    }

    private static readonly Regex PermissionFormat = new("^[a-z0-9_]+$", RegexOptions.Compiled);

    public void UpdatePermissions(IEnumerable<string> permissions)
    {
        Permissions = permissions
            .Select(p => p.Trim().ToLowerInvariant())
            .Where(p => PermissionFormat.IsMatch(p))
            .Distinct()
            .ToList();
        Touch();
    }
}
