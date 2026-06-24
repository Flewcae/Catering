using Catering.BuildingBlocks.Domain;

namespace Catering.UserService.Domain;

public sealed class Center : BaseEntity
{
    public Guid CenterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;

    private Center()
    {
    }

    public static Center Create(Guid centerId, string name, string address) =>
        new() { CenterId = centerId, Name = name, Address = address };

    public void UpdateFrom(string name, string address)
    {
        Name = name;
        Address = address;
        Touch();
    }
}
