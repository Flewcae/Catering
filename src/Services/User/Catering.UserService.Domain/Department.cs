using Catering.BuildingBlocks.Domain;

namespace Catering.UserService.Domain;

public sealed class Department : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Department()
    {
    }

    public static Department Create(string name, string? description) =>
        new() { Name = name, Description = description };

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        Touch();
    }
}
